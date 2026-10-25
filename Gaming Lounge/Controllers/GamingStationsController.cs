using Gaming_Lounge.Data;
using Gaming_Lounge.Models.Enums;
using Gaming_Lounge.Options;
using Gaming_Lounge.ViewModels.GamingStations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Gaming_Lounge.Controllers;

public class GamingStationsController(
    ApplicationDbContext context,
    IOptions<LoungeOptions> options) : Controller
{
    private readonly ApplicationDbContext _context = context;
    private readonly LoungeOptions _options = options.Value;

    public async Task<IActionResult> Index(int? categoryId, string? searchTerm)
    {
        var query = _context.GamingStations
            .AsNoTracking()
            .Include(x => x.GamingCategory)
            .Where(x => x.Status != StationStatus.Inactive && x.GamingCategory.IsActive);

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.GamingCategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(x => x.Name.Contains(term)
                || x.Description.Contains(term)
                || x.Specifications.Contains(term));
        }

        var stations = await query
            .OrderByDescending(x => x.IsFeatured)
            .ThenBy(x => x.Name)
            .Select(x => new GamingStationCardViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                Specifications = x.Specifications,
                HourlyRate = x.HourlyRate,
                ImagePath = x.ImagePath,
                CategoryName = x.GamingCategory.Name,
                CategoryAccent = x.GamingCategory.AccentColor,
                Status = x.Status
            })
            .ToListAsync();

        var categories = await _context.GamingCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new GamingCategoryFilterViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                IconClass = x.IconClass,
                AccentColor = x.AccentColor,
                StationCount = x.Stations.Count(s => s.Status != StationStatus.Inactive)
            })
            .ToListAsync();

        return View(new GamingStationsIndexViewModel
        {
            CategoryId = categoryId,
            SearchTerm = searchTerm,
            Categories = categories,
            Stations = stations
        });
    }

    public async Task<IActionResult> Details(string slug)
    {
        var station = await _context.GamingStations
            .AsNoTracking()
            .Include(x => x.GamingCategory)
            .SingleOrDefaultAsync(x => x.Slug == slug && x.Status != StationStatus.Inactive);
        if (station is null)
        {
            return NotFound();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        return View(new GamingStationDetailsViewModel
        {
            Id = station.Id,
            Name = station.Name,
            Description = station.Description,
            Specifications = station.Specifications,
            HourlyRate = station.HourlyRate,
            ImagePath = station.ImagePath,
            CategoryName = station.GamingCategory.Name,
            CategoryAccent = station.GamingCategory.AccentColor,
            Status = station.Status,
            EarliestDate = today,
            LatestDate = today.AddDays(_options.AdvanceBookingDays)
        });
    }
}
