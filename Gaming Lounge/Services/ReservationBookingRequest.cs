namespace Gaming_Lounge.Services;

public sealed record ReservationBookingRequest(
    int GamingStationId,
    DateOnly ReservationDate,
    TimeOnly StartTime,
    int DurationHours);
