using System.Data;
using Gaming_Lounge.Data;
using Gaming_Lounge.Models;
using Gaming_Lounge.Models.Enums;
using Gaming_Lounge.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Gaming_Lounge.Services;

public sealed class ReservationService(
    ApplicationDbContext context,
    IOptions<LoungeOptions> loungeOptions,
    TimeProvider timeProvider,
    ILogger<ReservationService> logger) : IReservationService
{
    private readonly ApplicationDbContext _context = context;
    private readonly LoungeOptions _options = loungeOptions.Value;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<ReservationService> _logger = logger;

    public async Task<IReadOnlyList<TimeOnly>> GetAvailableStartTimesAsync(
        int gamingStationId,
        DateOnly date,
        int durationHours,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateDateAndDuration(date, durationHours);
        if (validationError is not null)
        {
            return [];
        }

        var stationIsAvailable = await _context.GamingStations
            .AsNoTracking()
            .AnyAsync(x => x.Id == gamingStationId && x.Status == StationStatus.Available, cancellationToken);
        if (!stationIsAvailable)
        {
            return [];
        }

        var reservations = await _context.Reservations
            .AsNoTracking()
            .Where(x => x.GamingStationId == gamingStationId
                && x.ReservationDate == date
                && x.Status == ReservationStatus.Confirmed)
            .Select(x => new { x.StartTime, x.EndTime })
            .ToListAsync(cancellationToken);

        var available = new List<TimeOnly>();
        var open = TimeOnly.FromTimeSpan(_options.OpenTime);
        var close = TimeOnly.FromTimeSpan(_options.CloseTime);
        var localNow = GetLocalNow();

        for (var candidateOffset = _options.OpenTime;
             candidateOffset + TimeSpan.FromHours(durationHours) <= _options.CloseTime;
             candidateOffset += TimeSpan.FromHours(1))
        {
            var candidate = TimeOnly.FromTimeSpan(candidateOffset);
            var end = TimeOnly.FromTimeSpan(candidateOffset + TimeSpan.FromHours(durationHours));
            var isPast = date == DateOnly.FromDateTime(localNow) && candidate <= TimeOnly.FromDateTime(localNow);
            var conflicts = reservations.Any(x => ReservationRules.Overlaps(candidate, end, x.StartTime, x.EndTime));
            if (!isPast && !conflicts)
            {
                available.Add(candidate);
            }
        }

        return available;
    }

    public async Task<ServiceResult<int>> CreateAsync(
        string userId,
        ReservationBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ServiceResult<int>.Failure("You must be signed in to reserve a station.");
        }

        var validationError = ValidateDateAndDuration(request.ReservationDate, request.DurationHours);
        if (validationError is not null)
        {
            return ServiceResult<int>.Failure(validationError);
        }

        var requestedEnd = request.StartTime.AddHours(request.DurationHours);
        var open = TimeOnly.FromTimeSpan(_options.OpenTime);
        var close = TimeOnly.FromTimeSpan(_options.CloseTime);
        if (!ReservationRules.IsWithinOpeningHours(request.StartTime, requestedEnd, open, close))
        {
            return ServiceResult<int>.Failure($"Reservations must fit between {open:h:mm tt} and {close:h:mm tt}.");
        }

        var localNow = GetLocalNow();
        if (request.ReservationDate.ToDateTime(request.StartTime) <= localNow)
        {
            return ServiceResult<int>.Failure("Choose a reservation time in the future.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            GamingStation? station;
            if (_context.Database.IsSqlServer())
            {
                station = await _context.GamingStations
                    .FromSqlInterpolated($"SELECT * FROM [GamingStations] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {request.GamingStationId}")
                    .SingleOrDefaultAsync(cancellationToken);
            }
            else
            {
                station = await _context.GamingStations
                    .SingleOrDefaultAsync(x => x.Id == request.GamingStationId, cancellationToken);
            }

            if (station is null || station.Status != StationStatus.Available)
            {
                return ServiceResult<int>.Failure("This gaming station is currently unavailable.");
            }

            var hasConflict = await _context.Reservations.AnyAsync(x =>
                x.GamingStationId == request.GamingStationId
                && x.ReservationDate == request.ReservationDate
                && x.Status == ReservationStatus.Confirmed
                && x.StartTime < requestedEnd
                && request.StartTime < x.EndTime,
                cancellationToken);

            if (hasConflict)
            {
                return ServiceResult<int>.Failure("That time was just booked. Please choose another available time.");
            }

            var reservation = new Reservation
            {
                UserId = userId,
                GamingStationId = request.GamingStationId,
                ReservationDate = request.ReservationDate,
                StartTime = request.StartTime,
                EndTime = requestedEnd,
                DurationHours = request.DurationHours,
                HourlyRate = station.HourlyRate,
                TotalPrice = station.HourlyRate * request.DurationHours,
                Status = ReservationStatus.Confirmed
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ServiceResult<int>.Success(reservation.Id, "Your session is locked in.");
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(exception, "A reservation could not be saved for station {StationId}.", request.GamingStationId);
            return ServiceResult<int>.Failure("We could not complete the reservation. Please refresh and try again.");
        }
    }

    public async Task<ServiceResult> CancelAsync(
        int reservationId,
        string userId,
        bool isAdmin = false,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _context.Reservations
            .SingleOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
        if (reservation is null || (!isAdmin && reservation.UserId != userId))
        {
            return ServiceResult.Failure("Reservation not found.");
        }

        if (reservation.Status != ReservationStatus.Confirmed)
        {
            return ServiceResult.Failure("Only confirmed reservations can be cancelled.");
        }

        var cutoff = reservation.StartsAt.AddHours(-_options.CancellationCutoffHours);
        if (!isAdmin && GetLocalNow() >= cutoff)
        {
            return ServiceResult.Failure($"Reservations must be cancelled at least {_options.CancellationCutoffHours} hours before they begin.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAtUtc = _timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Reservation cancelled.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult.Failure("This reservation changed while you were viewing it. Please refresh and try again.");
        }
    }

    private string? ValidateDateAndDuration(DateOnly date, int durationHours)
    {
        var today = DateOnly.FromDateTime(GetLocalNow());
        if (date < today)
        {
            return "Reservations cannot be created in the past.";
        }

        if (date > today.AddDays(_options.AdvanceBookingDays))
        {
            return $"Reservations can be made up to {_options.AdvanceBookingDays} days ahead.";
        }

        if (durationHours < _options.MinimumDurationHours || durationHours > _options.MaximumDurationHours)
        {
            return $"Choose a duration between {_options.MinimumDurationHours} and {_options.MaximumDurationHours} hours.";
        }

        return null;
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
