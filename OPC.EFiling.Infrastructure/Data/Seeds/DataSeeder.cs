using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OPC.EFiling.Domain.Entities;
using System.Threading.Tasks;

namespace OPC.EFiling.Infrastructure.Data.Seeds
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<Role>>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            string[] roles = { "Admin", "RegistryOfficer", "Drafter", "SeniorPC", "MDAOfficer" };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new Role { Name = role });

            await EnsureUser(userManager, "admin@opc.ls", "Admin@123", "Admin");
            await EnsureUser(userManager, "registry@opc.ls", "Registry@123", "RegistryOfficer");
            await EnsureUser(userManager, "drafter@opc.ls", "Drafter@123", "Drafter");
            await EnsureUser(userManager, "senior@opc.ls", "Senior@123", "SeniorPC");
            await EnsureUser(userManager, "mda@ministry.ls", "Mda@123", "MDAOfficer");
        }

        private static async Task EnsureUser(UserManager<User> userManager, string email, string password, string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new User { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
