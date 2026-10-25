using System.ComponentModel.DataAnnotations;

namespace Gaming_Lounge.ViewModels.Account;

public sealed class RegisterViewModel
{
    [Required, StringLength(80)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Keep me signed in")]
    public bool RememberMe { get; set; }
}

public sealed class ProfileViewModel
{
    [Required, StringLength(80)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }
}
