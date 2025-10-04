using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OPC.EFiling.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OPC.EFiling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<Role> _roleManager;


        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (string.IsNullOrEmpty(model.Password))
                return BadRequest("Password cannot be null or empty."); // Ensure password is not null or empty

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                DepartmentID = model.DepartmentID,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign Role
            if (!string.IsNullOrEmpty(model.Role))
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                    return BadRequest($"Role '{model.Role}' does not exist.");

                await _userManager.AddToRoleAsync(user, model.Role);
            }

            return Ok("User registered successfully");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
                return BadRequest("Email cannot be null or empty.");

            if (string.IsNullOrEmpty(model.Password))
                return BadRequest("Password cannot be null or empty."); // Added null check for Password

            var user = await _userManager.FindByEmailAsync(model.Email ?? string.Empty); // Fix for CS8604
            if (user == null)
                return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized();

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var keyString = jwtSettings["Key"];

            if (string.IsNullOrEmpty(keyString))
                throw new InvalidOperationException("JWT key is not configured properly.");

            var key = Encoding.UTF8.GetBytes(keyString);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty), // Fix for CS8604
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add roles to claims
            var userRoles = _userManager.GetRolesAsync(user).Result;
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            if (string.IsNullOrEmpty(model.Email)) // Ensure Email is not null or empty
                return BadRequest("Email cannot be null or empty.");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Ok("If the email exists, an OTP has been sent."); // Avoid leaking info

            var otp = new Random().Next(10000, 99999).ToString();

            user.ResetOtp = otp;
            user.ResetOtpExpiry = DateTime.UtcNow.AddMinutes(10);

            await _userManager.UpdateAsync(user);

            // Simulate sending OTP (replace with email/SMS service)
            Console.WriteLine($"OTP for {user.Email}: {otp}");

            return Ok("If the email exists, an OTP has been sent.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithOtpModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
                return BadRequest("Email cannot be null or empty.");

            if (string.IsNullOrEmpty(model.Otp))
                return BadRequest("OTP cannot be null or empty.");

            if (string.IsNullOrEmpty(model.NewPassword))
                return BadRequest("New password cannot be null or empty."); // Ensure NewPassword is not null or empty

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Invalid request.");

            if (user.ResetOtp != model.Otp || user.ResetOtpExpiry < DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            user.ResetOtp = null;
            user.ResetOtpExpiry = null;
            await _userManager.UpdateAsync(user);

            return Ok("Password has been reset successfully.");
        }


    }
}
