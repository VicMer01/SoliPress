namespace DocumentApprovalSystem.Models.ViewModels;

public class UserWithRolesViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
    public string PrimaryRole => Roles.FirstOrDefault() ?? "Sin Rol";
}
