using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace IQGame.Admin
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            Console.WriteLine("[Seeder] Starting Admin role check...");
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                Console.WriteLine("[Seeder] Creating Admin role...");
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            else
            {
                Console.WriteLine("[Seeder] Admin role already exists.");
            }

            string adminEmail = "IQGame@admin.com";
            string adminPassword = "Aa12345678*";

            Console.WriteLine("[Seeder] Checking for admin user...");
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                Console.WriteLine("[Seeder] Creating admin user...");
                adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    Console.WriteLine("[Seeder] Failed to create admin user:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($" - {error.Code}: {error.Description}");
                    }
                    return;
                }
            }
            else
            {
                Console.WriteLine("[Seeder] Admin user already exists.");
            }

            Console.WriteLine("[Seeder] Checking admin role assignment...");
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                Console.WriteLine("[Seeder] Assigning Admin role to admin user...");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                Console.WriteLine("[Seeder] Admin user already in Admin role.");
            }

            Console.WriteLine("[Seeder] Admin seeding completed.");
        }
    }
}