using Microsoft.AspNetCore.Identity;
using UserRoleAuth.Core.Entities;

namespace UserRoleAuth.Api.Extensions
{
    public static class IdentitySeedExtensions
    {
        public static async Task SeedAdminUser(this IHost app)
        {
            using var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Ensure Admin role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = "Admin",
                    Description = "Administrator role"
                });
            }

            // 2. Ensure Admin user exists
            var adminUser = await userManager.FindByNameAsync("admin");

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    DisplayName = "Administrator"
                };

                await userManager.CreateAsync(adminUser, "Admin@123");
            }

            // 3. Ensure Admin user has Admin role ALWAYS
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
