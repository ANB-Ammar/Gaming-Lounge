using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Gaming_Lounge.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(80)]
    public string DisplayName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Reservation> Reservations { get; set; } = [];
}
