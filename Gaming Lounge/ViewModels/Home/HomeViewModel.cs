using Gaming_Lounge.ViewModels.GamingStations;

namespace Gaming_Lounge.ViewModels.Home;

public sealed class HomeViewModel
{
    public IReadOnlyList<GamingStationCardViewModel> FeaturedStations { get; init; } = [];
    public IReadOnlyList<GamingCategoryFilterViewModel> Categories { get; init; } = [];
    public int ActiveStationCount { get; init; }
    public int CategoryCount { get; init; }
}
