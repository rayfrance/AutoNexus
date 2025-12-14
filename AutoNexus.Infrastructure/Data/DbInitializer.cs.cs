using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using AutoNexus.Domain;

namespace AutoNexus.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await CreateRoles(roleManager);
            await CreateAdminUser(userManager);
            await CreateSellerUser(userManager);
        }
        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in Constants.AllRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
        private static async Task CreateSellerUser(UserManager<IdentityUser> userManager)
        {
            if (await userManager.FindByEmailAsync(Constants.SellerEmail) != null)
                return;

            IdentityUser sellerUser = CreateUser(Constants.SellerEmail);

            await userManager.CreateAsync(sellerUser, Constants.SellerPassword);
            await userManager.AddToRoleAsync(sellerUser, Constants.SellerRole);
        }
        private static async Task CreateAdminUser(UserManager<IdentityUser> userManager)
        {
            if (await userManager.FindByEmailAsync(Constants.AdminEmail) != null)
                return;

            IdentityUser adminUser = CreateUser(Constants.AdminEmail);

            await userManager.CreateAsync(adminUser, Constants.AdminPassword);
            await userManager.AddToRoleAsync(adminUser, Constants.AdminRole);
        }
        private static IdentityUser CreateUser(string userEmail)
        {
            return new()
            {
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true
            };
        }

    }
}