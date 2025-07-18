using Microsoft.AspNetCore.Identity;
using OPC.EFiling.Domain.Entities;

namespace OPC.EFiling.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAsync(RoleManager<Role> roleManager)
        {
            var roles = new List<string> { "Admin", "Drafter", "RegistryOfficer" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role { Name = roleName });
                }
            }
        }

        public static async Task SeedAdminUserAsync(UserManager<User> userManager)
        {
            var adminEmail = "admin@opc.gov.ls";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    DepartmentID = 1, // You can adjust department ID
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(newAdmin, "Admin123!");

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}
