using Microsoft.AspNetCore.Identity;
using Moviest.Constants;

namespace Moviest.Data
{
    public class IdentitySeedData
    {
        public static async Task SeedDataAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            await EnsureRolesAsync(roleManager);
            await EnsureAdminAsync(userManager, configuration);
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));

            if (!await roleManager.RoleExistsAsync(Roles.User))
                await roleManager.CreateAsync(new IdentityRole(Roles.User));
        }

        private static async Task EnsureAdminAsync(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            var adminEmail = configuration["AdminCredentials:Email"];
            var adminPassword = configuration["AdminCredentials:Password"];

            var adminUser = await userManager.FindByNameAsync(Roles.Admin);

            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = Roles.Admin, Email = adminEmail };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
            else if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
        }
    }
}
