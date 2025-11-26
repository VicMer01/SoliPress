using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DocumentApprovalSystem.Models;
using DocumentApprovalSystem.Models.ViewModels;

namespace DocumentApprovalSystem.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<List<UserWithRolesViewModel>> GetAllUsersWithRolesAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var usersWithRoles = new List<UserWithRolesViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            usersWithRoles.Add(new UserWithRolesViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Department = user.Department,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.Now,
                Roles = roles.ToList()
            });
        }

        return usersWithRoles;
    }

    public async Task<UserWithRolesViewModel?> GetUserWithRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserWithRolesViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Department = user.Department,
            EmailConfirmed = user.EmailConfirmed,
            IsActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.Now,
            Roles = roles.ToList()
        };
    }

    public async Task<(bool Success, string[] Errors)> CreateUserAsync(CreateUserViewModel model)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return (false, new[] { "Ya existe un usuario con este email" });
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Department = model.Department,
                EmailConfirmed = model.EmailConfirmed,
                Role = model.Role // For compatibility
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }

            // Assign role
            if (!string.IsNullOrEmpty(model.Role))
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
            }

            _logger.LogInformation("User created successfully: {Email}", model.Email);
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return (false, new[] { "Error al crear el usuario" });
        }
    }

    public async Task<(bool Success, string[] Errors)> UpdateUserAsync(EditUserViewModel model)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return (false, new[] { "Usuario no encontrado" });
            }

            // Update basic info
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.Department = model.Department;
            user.EmailConfirmed = model.EmailConfirmed;
            user.Role = model.Role; // For compatibility

            // Update lockout status
            user.LockoutEnabled = !model.IsActive;
            if (!model.IsActive)
            {
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
            else
            {
                user.LockoutEnd = null;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return (false, updateResult.Errors.Select(e => e.Description).ToArray());
            }

            // Update role
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            
            if (!string.IsNullOrEmpty(model.Role))
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    return (false, passwordResult.Errors.Select(e => e.Description).ToArray());
                }
            }

            _logger.LogInformation("User updated successfully: {Email}", model.Email);
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return (false, new[] { "Error al actualizar el usuario" });
        }
    }

    public async Task<(bool Success, string[] Errors)> DeleteUserAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, new[] { "Usuario no encontrado" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }

            _logger.LogInformation("User deleted successfully: {Email}", user.Email);
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            return (false, new[] { "Error al eliminar el usuario" });
        }
    }

    public async Task<(bool Success, string[] Errors)> ToggleUserStatusAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, new[] { "Usuario no encontrado" });
            }

            // Toggle lockout
            var isCurrentlyActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.Now;
            
            if (isCurrentlyActive)
            {
                // Deactivate user
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
            else
            {
                // Activate user
                user.LockoutEnabled = false;
                user.LockoutEnd = null;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }

            _logger.LogInformation("User status toggled: {Email}, Active: {IsActive}", user.Email, !isCurrentlyActive);
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status");
            return (false, new[] { "Error al cambiar el estado del usuario" });
        }
    }

    public async Task<(bool Success, string[] Errors)> ResetPasswordAsync(string userId, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, new[] { "Usuario no encontrado" });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }

            _logger.LogInformation("Password reset for user: {Email}", user.Email);
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return (false, new[] { "Error al resetear la contraseña" });
        }
    }

    public async Task<List<string>> GetAllRolesAsync()
    {
        return await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
    }
}
