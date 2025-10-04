using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using OPC.EFiling.Domain.Entities;

// NOTE: This controller adds the ability for an existing admin to create
// additional admin accounts.  Only users with the "Admin" role may access
// these endpoints.  The controller relies on ASP.NET Core Identity's
// UserManager and RoleManager to manage users and roles.

namespace OPC.EFiling.API.Controllers
{
    [ApiController]
    // Route prefix; e.g. POST /api/admin/create
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        // Use the application's custom user and role types (IdentityUser<int> and IdentityRole<int>)
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public AdminController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Creates a new admin user.  Requires the caller to be in the
        /// "Admin" role.  If the admin role does not exist, it will be
        /// created automatically.
        /// </summary>
        /// <param name="input">User details (e‑mail and password).</param>
        [HttpPost("create")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto input)
        {
            if (string.IsNullOrWhiteSpace(input?.Email) || string.IsNullOrWhiteSpace(input?.Password))
            {
                return BadRequest("Email and password are required.");
            }

            // Ensure the Admin role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new Role { Name = "Admin" });
            }

            var user = new User
            {
                UserName = input.Email,
                Email = input.Email,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, input.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "Admin");
            return Ok();
        }

        // DTO used for creating an admin user.  In a real application this
        // might include additional profile information.
        public record CreateAdminDto(string Email, string Password);
    }
}