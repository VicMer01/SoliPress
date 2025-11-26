using Microsoft.AspNetCore.Identity;
using DocumentApprovalSystem.Models;

namespace DocumentApprovalSystem.Data;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Administrador", "Solicitante", "Aprobador" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        // Check if admin user already exists
        var adminEmail = "admin@documentapproval.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            // Create admin user
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrador del Sistema",
                Department = "Administración",
                EmailConfirmed = true,
                Role = "Administrador" // For compatibility with existing code
            };

            var password = "Admin@123";
            var result = await userManager.CreateAsync(adminUser, password);

            if (result.Succeeded)
            {
                // Assign Administrador role
                await userManager.AddToRoleAsync(adminUser, "Administrador");
                logger.LogInformation("Admin user created successfully: {Email}", adminEmail);
                logger.LogInformation("Default password: {Password}", password);
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists: {Email}", adminEmail);
        }
    }
}
