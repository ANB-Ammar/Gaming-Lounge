using Gaming_Lounge.Data;
using Gaming_Lounge.Models;
using Gaming_Lounge.Models.Enums;
using Gaming_Lounge.Services;
using Gaming_Lounge.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Gaming_Lounge.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class StationsController(ApplicationDbContext context) : Controller
{
    private const int SlugMaxLength = 120;
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, StationStatus? status)
    {
        var normalizedSearch = searchTerm?.Trim() ?? string.Empty;
        var query = _context.GamingStations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(station => station.Name.Contains(normalizedSearch)
                || station.Slug.Contains(normalizedSearch));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(station => station.GamingCategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(station => station.Status == status.Value);
        }

        var stations = await query
            .OrderBy(station => station.GamingCategory.SortOrder)
            .ThenBy(station => station.Name)
            .Select(station => new AdminStationRowViewModel
            {
                Id = station.Id,
                Name = station.Name,
                Slug = station.Slug,
                ImagePath = station.ImagePath,
                CategoryName = station.GamingCategory.Name,
                HourlyRate = station.HourlyRate,
                Status = station.Status,
                IsFeatured = station.IsFeatured,
                ReservationCount = station.Reservations.Count
            })
            .ToListAsync();

        var categories = await _context.GamingCategories
            .AsNoTracking()
            .OrderBy(category => category.SortOrder)
            .Select(category => new SelectListItem(category.Name, category.Id.ToString()))
            .ToListAsync();

        return View(new AdminStationsIndexViewModel
        {
            SearchTerm = normalizedSearch,
            CategoryId = categoryId,
            Status = status,
            Categories = categories,
            Stations = stations
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new AdminStationEditViewModel();
        await PopulateCategoriesAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminStationEditViewModel model)
    {
        ModelState.Remove(nameof(model.Slug));
        model.Slug = SlugGenerator.Generate(model.Name, SlugMaxLength, "station");

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(model);
            return View(model);
        }

        var station = new GamingStation
        {
            Name = model.Name.Trim(),
            Description = model.Description.Trim(),
            Specifications = model.Specifications.Trim(),
            HourlyRate = model.HourlyRate,
            ImagePath = model.ImagePath.Trim(),
            GamingCategoryId = model.GamingCategoryId,
            Status = model.Status,
            IsFeatured = model.IsFeatured
        };

        if (!await TryCreateWithUniqueSlugAsync(station))
        {
            ModelState.AddModelError(string.Empty, "A unique station URL could not be generated. Please try again.");
            model.Slug = station.Slug;
            await PopulateCategoriesAsync(model);
            return View(model);
        }

        TempData["Success"] = "Gaming station created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var station = await _context.GamingStations.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
        if (station is null)
        {
            return NotFound();
        }

        var model = new AdminStationEditViewModel
        {
            Id = station.Id,
            Name = station.Name,
            Slug = station.Slug,
            Description = station.Description,
            Specifications = station.Specifications,
            HourlyRate = station.HourlyRate,
            ImagePath = station.ImagePath,
            GamingCategoryId = station.GamingCategoryId,
            Status = station.Status,
            IsFeatured = station.IsFeatured,
            RowVersion = Convert.ToBase64String(station.RowVersion)
        };
        await PopulateCategoriesAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminStationEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var station = await _context.GamingStations.SingleOrDefaultAsync(item => item.Id == id);
        if (station is null)
        {
            return NotFound();
        }

        ModelState.Remove(nameof(model.Slug));
        model.Slug = station.Slug;

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(model);
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.RowVersion))
        {
            _context.Entry(station).Property(item => item.RowVersion).OriginalValue =
                Convert.FromBase64String(model.RowVersion);
        }

        station.Name = model.Name.Trim();
        station.Description = model.Description.Trim();
        station.Specifications = model.Specifications.Trim();
        station.HourlyRate = model.HourlyRate;
        station.ImagePath = model.ImagePath.Trim();
        station.GamingCategoryId = model.GamingCategoryId;
        station.Status = model.Status;
        station.IsFeatured = model.IsFeatured;

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Gaming station updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError(string.Empty, "Another admin updated this station. Reload the page and try again.");
            await PopulateCategoriesAsync(model);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleMaintenance(int id)
    {
        var station = await _context.GamingStations.SingleOrDefaultAsync(item => item.Id == id);
        if (station is null)
        {
            return NotFound();
        }

        station.Status = station.Status == StationStatus.Maintenance
            ? StationStatus.Available
            : StationStatus.Maintenance;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"{station.Name} is now {station.Status}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var station = await _context.GamingStations
            .AsNoTracking()
            .Include(item => item.GamingCategory)
            .SingleOrDefaultAsync(item => item.Id == id);
        return station is null ? NotFound() : View(station);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var station = await _context.GamingStations.SingleOrDefaultAsync(item => item.Id == id);
        if (station is null)
        {
            return NotFound();
        }

        if (await _context.Reservations.AnyAsync(reservation => reservation.GamingStationId == id))
        {
            TempData["Error"] = "Stations with reservation history cannot be deleted. Mark it inactive instead.";
            return RedirectToAction(nameof(Index));
        }

        _context.GamingStations.Remove(station);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Gaming station deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> TryCreateWithUniqueSlugAsync(GamingStation station)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var existingSlugs = await _context.GamingStations
                .AsNoTracking()
                .Select(item => item.Slug)
                .ToListAsync();

            station.Slug = SlugGenerator.GenerateUnique(
                station.Name,
                existingSlugs,
                SlugMaxLength,
                "station");

            _context.GamingStations.Add(station);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
            {
                _context.Entry(station).State = EntityState.Detached;
            }
        }

        return false;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    private async Task PopulateCategoriesAsync(AdminStationEditViewModel model)
    {
        model.Categories = await _context.GamingCategories
            .AsNoTracking()
            .OrderBy(category => category.SortOrder)
            .Select(category => new SelectListItem(category.Name, category.Id.ToString()))
            .ToListAsync();
    }
}