using Gaming_Lounge.Data;
using Gaming_Lounge.Models;
using Gaming_Lounge.Services;
using Gaming_Lounge.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Gaming_Lounge.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController(ApplicationDbContext context) : Controller
{
    private const int SlugMaxLength = 80;
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index() => View(await _context.GamingCategories
        .AsNoTracking()
        .Include(category => category.Stations)
        .OrderBy(category => category.SortOrder)
        .ToListAsync());

    [HttpGet]
    public IActionResult Create() => View(new AdminCategoryEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminCategoryEditViewModel model)
    {
        ModelState.Remove(nameof(model.Slug));
        model.Slug = SlugGenerator.Generate(model.Name, SlugMaxLength, "category");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var category = new GamingCategory
        {
            Name = model.Name.Trim(),
            Description = model.Description.Trim(),
            IconClass = model.IconClass.Trim(),
            AccentColor = model.AccentColor.Trim().ToLowerInvariant(),
            IsActive = model.IsActive,
            SortOrder = model.SortOrder
        };

        if (!await TryCreateWithUniqueSlugAsync(category))
        {
            ModelState.AddModelError(string.Empty, "A unique category URL could not be generated. Please try again.");
            model.Slug = category.Slug;
            return View(model);
        }

        TempData["Success"] = "Category created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.GamingCategories.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
        return category is null ? NotFound() : View(new AdminCategoryEditViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            IconClass = category.IconClass,
            AccentColor = category.AccentColor,
            IsActive = category.IsActive,
            SortOrder = category.SortOrder
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminCategoryEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var category = await _context.GamingCategories.SingleOrDefaultAsync(item => item.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        ModelState.Remove(nameof(model.Slug));
        model.Slug = category.Slug;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        category.Name = model.Name.Trim();
        category.Description = model.Description.Trim();
        category.IconClass = model.IconClass.Trim();
        category.AccentColor = model.AccentColor.Trim().ToLowerInvariant();
        category.IsActive = model.IsActive;
        category.SortOrder = model.SortOrder;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Category updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.GamingCategories
            .Include(item => item.Stations)
            .SingleOrDefaultAsync(item => item.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        if (category.Stations.Count > 0)
        {
            TempData["Error"] = "Move or remove this category's stations before deleting it.";
            return RedirectToAction(nameof(Index));
        }

        _context.GamingCategories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Category deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> TryCreateWithUniqueSlugAsync(GamingCategory category)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var existingSlugs = await _context.GamingCategories
                .AsNoTracking()
                .Select(item => item.Slug)
                .ToListAsync();

            category.Slug = SlugGenerator.GenerateUnique(
                category.Name,
                existingSlugs,
                SlugMaxLength,
                "category");

            _context.GamingCategories.Add(category);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
            {
                _context.Entry(category).State = EntityState.Detached;
            }
        }

        return false;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}