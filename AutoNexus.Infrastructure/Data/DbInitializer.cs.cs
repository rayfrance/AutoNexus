using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AutoNexus.Infrastructure.Data
{
    public static class DbInitializer
    {
        private const string ADMIN_EMAIL = "admin@autonexus.com";
        private const string SELLER_EMAIL = "vendedor@autonexus.com";
        private const string SELLER_PASSWORD = "Vendedor@123";
        private const string ADMIN_PASSWORD = "Admin@123";

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await CreateRoles(roleManager);

            await CreateAdminUser(userManager);

            await CreateSellerUser(userManager);
        }

        private static async Task CreateSellerUser(UserManager<IdentityUser> userManager)
        {
            if (await userManager.FindByEmailAsync(SELLER_EMAIL)! == null)
                return;

            IdentityUser seller = new() { UserName = SELLER_EMAIL, Email = SELLER_EMAIL, EmailConfirmed = true };
            await userManager.CreateAsync(seller, SELLER_PASSWORD);
            await userManager.AddToRoleAsync(seller, "Vendedor");
        }

        private static async Task CreateAdminUser(UserManager<IdentityUser> userManager)
        {
            if (await userManager.FindByEmailAsync(ADMIN_EMAIL) != null)
                return;

            IdentityUser admin = new() { UserName = ADMIN_EMAIL, Email = ADMIN_EMAIL, EmailConfirmed = true };
            await userManager.CreateAsync(admin, ADMIN_PASSWORD);
            await userManager.AddToRoleAsync(admin, "Admin");
            
        }

        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Vendedor" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}