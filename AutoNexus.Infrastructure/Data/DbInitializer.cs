using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using AutoNexus.Domain;
using AutoNexus.Domain.Entities; 
using AutoNexus.Domain.Enums;    

namespace AutoNexus.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            await SeedIdentitiesAsync(userManager, roleManager);
            await SeedVehiclesAsync(context);
        }

        private static async Task SeedIdentitiesAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await CreateRolesAsync(roleManager);
            await CreateAdminUserAsync(userManager);
            await CreateSellerUserAsync(userManager);
        }

        private static async Task SeedVehiclesAsync(ApplicationDbContext context)
        {
            if (context.Vehicles.Any()) return;

            // 1. Criar Fabricantes
            Manufacturer[] manufacturers = await CreateManufacturerAsync(context);
            // 2. Criar Veículos (Vinculando aos fabricantes criados acima)
            await CreateVehiclesAsync(context, manufacturers);
        }

        private static async Task CreateVehiclesAsync(ApplicationDbContext context, Manufacturer[] manufacturers)
        {
            var vehicles = new Vehicle[]
            {
                new Vehicle { Model = "Corolla XEi", Year = 2022, Price = 145000, Status = VehicleStatus.Available, ManufacturerId = manufacturers[0].Id },
                new Vehicle { Model = "Civic Touring", Year = 2021, Price = 160000, Status = VehicleStatus.Available, ManufacturerId = manufacturers[1].Id },
                new Vehicle { Model = "Mustang GT", Year = 2020, Price = 450000, Status = VehicleStatus.Sold, ManufacturerId = manufacturers[2].Id },
                new Vehicle { Model = "Onix Plus", Year = 2023, Price = 85000, Status = VehicleStatus.Available, ManufacturerId = manufacturers[3].Id },
                new Vehicle { Model = "320i M Sport", Year = 2024, Price = 320000, Status = VehicleStatus.Reserved, ManufacturerId = manufacturers[4].Id },
                new Vehicle { Model = "Yaris Hatch", Year = 2023, Price = 98000, Status = VehicleStatus.Available, ManufacturerId = manufacturers[0].Id },
                new Vehicle { Model = "City Hatch", Year = 2024, Price = 115000, Status = VehicleStatus.Available, ManufacturerId = manufacturers[1].Id }
            };

            await context.Vehicles.AddRangeAsync(vehicles);
            await context.SaveChangesAsync();
        }

        private static async Task<Manufacturer[]> CreateManufacturerAsync(ApplicationDbContext context)
        {
            var manufacturers = new Manufacturer[]
            {
                new Manufacturer { Name = "Toyota" },
                new Manufacturer { Name = "Honda" },
                new Manufacturer { Name = "Ford" },
                new Manufacturer { Name = "Chevrolet" },
                new Manufacturer { Name = "BMW" }
            };

            await context.Manufacturers.AddRangeAsync(manufacturers);
            await context.SaveChangesAsync();
            return manufacturers;
        }

        private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in Constants.AllRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task CreateSellerUserAsync(UserManager<IdentityUser> userManager)
        {
            if (await userManager.FindByEmailAsync(Constants.SELLER_EMAIL) != null) return;
            IdentityUser sellerUser = CreateUser(Constants.SELLER_EMAIL);
            await userManager.CreateAsync(sellerUser, Constants.SELLER_PASSWORD);
            await userManager.AddToRoleAsync(sellerUser, Constants.SELLER_ROLE);
        }

        private static async Task CreateAdminUserAsync(UserManager<IdentityUser> userManager)
        {
            if (await userManager.FindByEmailAsync(Constants.ADMIN_EMAIL) != null) return;
            IdentityUser adminUser = CreateUser(Constants.ADMIN_EMAIL);
            await userManager.CreateAsync(adminUser, Constants.ADMIN_PASSWORD);
            await userManager.AddToRoleAsync(adminUser, Constants.ADMIN_ROLE);
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