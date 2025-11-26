using DocumentApprovalSystem.Models;
using DocumentApprovalSystem.Models.ViewModels;

namespace DocumentApprovalSystem.Services;

public interface IUserManagementService
{
    Task<List<UserWithRolesViewModel>> GetAllUsersWithRolesAsync();
    Task<UserWithRolesViewModel?> GetUserWithRolesAsync(string userId);
    Task<(bool Success, string[] Errors)> CreateUserAsync(CreateUserViewModel model);
    Task<(bool Success, string[] Errors)> UpdateUserAsync(EditUserViewModel model);
    Task<(bool Success, string[] Errors)> DeleteUserAsync(string userId);
    Task<(bool Success, string[] Errors)> ToggleUserStatusAsync(string userId);
    Task<(bool Success, string[] Errors)> ResetPasswordAsync(string userId, string newPassword);
    Task<List<string>> GetAllRolesAsync();
}
