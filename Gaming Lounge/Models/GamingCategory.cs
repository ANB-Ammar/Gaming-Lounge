using System.ComponentModel.DataAnnotations;

namespace Gaming_Lounge.Models;

public class GamingCategory
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Slug { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string IconClass { get; set; } = "bi-controller";

    [Required, StringLength(20)]
    public string AccentColor { get; set; } = "cyan";

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<GamingStation> Stations { get; set; } = [];
}
