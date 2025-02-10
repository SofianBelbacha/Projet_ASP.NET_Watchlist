using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Watchlist.Data;
using Watchlist.Models;

public class IdentitySeeder
{
    public static async Task SeedRolesAndAdmin(IServiceProvider serviceProvider, ILogger<IdentitySeeder> logger)
    {
        try
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Utilisateur>>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation($"Rôle {roleName} créé.");
                }
            }

            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@watchlist.com";
            string adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "5WS2Kb£?u2(3";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var admin = new Utilisateur
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createAdmin = await userManager.CreateAsync(admin, adminPassword);

                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    logger.LogInformation("Administrateur créé avec succès.");
                }
                else
                {
                    foreach (var error in createAdmin.Errors)
                    {
                        logger.LogError($"Erreur lors de la création de l'admin : {error.Description}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Erreur lors de l'initialisation d'Identity : {ex.Message}");
        }
    }
}
