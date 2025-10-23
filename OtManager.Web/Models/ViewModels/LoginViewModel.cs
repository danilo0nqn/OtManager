using System.ComponentModel.DataAnnotations;

namespace OtManager.Web.Models.ViewModels;

public sealed class LoginViewModel
{
    [Required]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordar usuario")]
    public bool RememberMe { get; set; }

    public string Theme { get; set; } = "light";
}
