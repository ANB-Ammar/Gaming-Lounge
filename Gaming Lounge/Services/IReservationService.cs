namespace Gaming_Lounge.Services;

public interface IReservationService
{
    Task<IReadOnlyList<TimeOnly>> GetAvailableStartTimesAsync(
        int gamingStationId,
        DateOnly date,
        int durationHours,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<int>> CreateAsync(
        string userId,
        ReservationBookingRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> CancelAsync(
        int reservationId,
        string userId,
        bool isAdmin = false,
        CancellationToken cancellationToken = default);
}
