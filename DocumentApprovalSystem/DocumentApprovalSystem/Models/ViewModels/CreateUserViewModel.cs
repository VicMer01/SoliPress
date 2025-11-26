using System.ComponentModel.DataAnnotations;

namespace DocumentApprovalSystem.Models.ViewModels;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
    public string? Department { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un rol")]
    public string Role { get; set; } = "Solicitante";

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe confirmar la contraseña")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool EmailConfirmed { get; set; } = true;
}
