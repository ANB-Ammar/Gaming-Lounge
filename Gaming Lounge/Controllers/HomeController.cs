using System.Diagnostics;
using Gaming_Lounge.Data;
using Gaming_Lounge.Models;
using Gaming_Lounge.Models.Enums;
using Gaming_Lounge.ViewModels.GamingStations;
using Gaming_Lounge.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gaming_Lounge.Controllers;

public class HomeController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index()
    {
        var featured = await _context.GamingStations
            .AsNoTracking()
            .Where(x => x.IsFeatured && x.Status != StationStatus.Inactive)
            .OrderBy(x => x.GamingCategory.SortOrder)
            .Take(3)
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

        return View(new HomeViewModel
        {
            FeaturedStations = featured,
            Categories = categories,
            ActiveStationCount = await _context.GamingStations.CountAsync(x => x.Status == StationStatus.Available),
            CategoryCount = categories.Count
        });
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
