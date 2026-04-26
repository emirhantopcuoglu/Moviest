using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moviest.Constants;
using static Moviest.Constants.ConfigKeys;

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
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentitySeedData>>();

            await EnsureRolesAsync(roleManager);
            await EnsureAdminAsync(userManager, configuration, logger);
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));

            if (!await roleManager.RoleExistsAsync(Roles.User))
                await roleManager.CreateAsync(new IdentityRole(Roles.User));
        }

        private static async Task EnsureAdminAsync(
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            ILogger<IdentitySeedData> logger)
        {
            var adminUser = await userManager.FindByNameAsync(Roles.Admin);
            if (adminUser != null)
            {
                if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                return;
            }

            var adminEmail = configuration[AdminEmail];
            var adminPassword = configuration[AdminPassword];
            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    $"'{AdminEmail}' ve '{AdminPassword}' yapılandırmaları, ilk admin hesabını oluşturmak için zorunludur.");
            }

            adminUser = new IdentityUser
            {
                UserName = Roles.Admin,
                Email = adminEmail
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(error => error.Description));
                logger.LogError("Admin kullanıcısı oluşturulamadı: {Errors}", errors);
                throw new InvalidOperationException($"Admin kullanıcısı oluşturulamadı: {errors}");
            }

            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }
    }
}
