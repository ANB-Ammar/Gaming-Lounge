using Gaming_Lounge.Data;
using Gaming_Lounge.Models.Enums;
using Gaming_Lounge.Options;
using Gaming_Lounge.Services;
using Gaming_Lounge.ViewModels.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Gaming_Lounge.Models;

namespace Gaming_Lounge.Controllers;

public class ReservationsController(
    ApplicationDbContext context,
    IReservationService reservationService,
    UserManager<ApplicationUser> userManager,
    IOptions<LoungeOptions> options,
    TimeProvider timeProvider) : Controller
{
    private readonly ApplicationDbContext _context = context;
    private readonly IReservationService _reservationService = reservationService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly LoungeOptions _options = options.Value;
    private readonly TimeProvider _timeProvider = timeProvider;

    [HttpGet]
    public async Task<IActionResult> Create(int stationId, DateOnly? date = null)
    {
        var model = new ReservationCreateViewModel
        {
            GamingStationId = stationId,
            ReservationDate = date ?? DateOnly.FromDateTime(GetLocalNow()),
            DurationHours = 1
        };

        return await PopulateCreateViewAsync(model) ? View(model) : NotFound();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationCreateViewModel model)
    {
        if (!ModelState.IsValid || model.ReservationDate is null || model.StartTime is null)
        {
            if (!await PopulateCreateViewAsync(model))
            {
                return NotFound();
            }

            return View(model);
        }

        var userId = _userManager.GetUserId(User)!;
        var result = await _reservationService.CreateAsync(userId, new ReservationBookingRequest(
            model.GamingStationId,
            model.ReservationDate.Value,
            model.StartTime.Value,
            model.DurationHours));

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await PopulateCreateViewAsync(model);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Confirmation), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Availability(int stationId, DateOnly date, int durationHours)
    {
        var slots = await _reservationService.GetAvailableStartTimesAsync(stationId, date, durationHours);
        return Json(slots.Select(x => new
        {
            value = x.ToString("HH:mm"),
            label = x.ToString("h:mm tt")
        }));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var reservation = await _context.Reservations
            .AsNoTracking()
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new ReservationConfirmationViewModel
            {
                Id = x.Id,
                StationName = x.GamingStation.Name,
                CategoryName = x.GamingStation.GamingCategory.Name,
                ReservationDate = x.ReservationDate,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                DurationHours = x.DurationHours,
                HourlyRate = x.HourlyRate,
                TotalPrice = x.TotalPrice
            })
            .SingleOrDefaultAsync();

        return reservation is null ? NotFound() : View(reservation);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        var userId = _userManager.GetUserId(User)!;
        var now = GetLocalNow();
        var reservations = await _context.Reservations
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ReservationDate)
            .ThenByDescending(x => x.StartTime)
            .Select(x => new ReservationListItemViewModel
            {
                Id = x.Id,
                StationName = x.GamingStation.Name,
                StationImagePath = x.GamingStation.ImagePath,
                CategoryName = x.GamingStation.GamingCategory.Name,
                ReservationDate = x.ReservationDate,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                DurationHours = x.DurationHours,
                TotalPrice = x.TotalPrice,
                Status = x.Status
            })
            .ToListAsync();

        var items = reservations.Select(x => new ReservationListItemViewModel
        {
            Id = x.Id,
            StationName = x.StationName,
            StationImagePath = x.StationImagePath,
            CategoryName = x.CategoryName,
            ReservationDate = x.ReservationDate,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            DurationHours = x.DurationHours,
            TotalPrice = x.TotalPrice,
            Status = x.Status,
            CanCancel = x.Status == ReservationStatus.Confirmed
                && x.ReservationDate.ToDateTime(x.StartTime) > now.AddHours(_options.CancellationCutoffHours)
        }).ToList();

        return View(new ReservationHistoryViewModel
        {
            Upcoming = items
                .Where(x => x.Status == ReservationStatus.Confirmed && x.ReservationDate.ToDateTime(x.StartTime) > now)
                .OrderBy(x => x.ReservationDate)
                .ThenBy(x => x.StartTime)
                .ToList(),
            Previous = items
                .Where(x => x.Status != ReservationStatus.Confirmed || x.ReservationDate.ToDateTime(x.StartTime) <= now)
                .ToList()
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var result = await _reservationService.CancelAsync(id, userId);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Mine));
    }

    private async Task<bool> PopulateCreateViewAsync(ReservationCreateViewModel model)
    {
        var station = await _context.GamingStations
            .AsNoTracking()
            .Include(x => x.GamingCategory)
            .SingleOrDefaultAsync(x => x.Id == model.GamingStationId && x.Status != StationStatus.Inactive);
        if (station is null)
        {
            return false;
        }

        var today = DateOnly.FromDateTime(GetLocalNow());
        model.StationName = station.Name;
        model.StationImagePath = station.ImagePath;
        model.CategoryName = station.GamingCategory.Name;
        model.HourlyRate = station.HourlyRate;
        model.EarliestDate = today;
        model.LatestDate = today.AddDays(_options.AdvanceBookingDays);
        return true;
    }

    private DateTime GetLocalNow()
    {
        try
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZoneId);
            return TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), zone).DateTime;
        }
        catch (TimeZoneNotFoundException)
        {
            return _timeProvider.GetLocalNow().DateTime;
        }
    }
}
