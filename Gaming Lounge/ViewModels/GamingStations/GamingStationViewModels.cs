using Gaming_Lounge.Models.Enums;

namespace Gaming_Lounge.ViewModels.GamingStations;

public sealed class GamingStationCardViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Specifications { get; init; } = string.Empty;
    public decimal HourlyRate { get; init; }
    public string ImagePath { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryAccent { get; init; } = string.Empty;
    public StationStatus Status { get; init; }
}

public sealed class GamingCategoryFilterViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string IconClass { get; init; } = string.Empty;
    public string AccentColor { get; init; } = string.Empty;
    public int StationCount { get; init; }
}

public sealed class GamingStationsIndexViewModel
{
    public int? CategoryId { get; init; }
    public string? SearchTerm { get; init; }
    public IReadOnlyList<GamingCategoryFilterViewModel> Categories { get; init; } = [];
    public IReadOnlyList<GamingStationCardViewModel> Stations { get; init; } = [];
}

public sealed class GamingStationDetailsViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Specifications { get; init; } = string.Empty;
    public decimal HourlyRate { get; init; }
    public string ImagePath { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryAccent { get; init; } = string.Empty;
    public StationStatus Status { get; init; }
    public DateOnly EarliestDate { get; init; }
    public DateOnly LatestDate { get; init; }
}
