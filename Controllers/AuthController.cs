using Auth.Data;
using Auth.Model.DTOs;
using Auth.Model.DTOs.AuthApi.Models.Dtos;
using Auth.Model.Entities;
using Auth.Services;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using System.Security.Claims;
using System.Security.Cryptography;
namespace Auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _db;

        public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService, ApplicationDbContext db, SignInManager<ApplicationUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _db = db;
            _signInManager = signInManager;
            _emailService = emailService;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                return BadRequest(new
                {
                    message = "Email is already registered."
                });
            }
            var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Student"); // default role
            return Ok(new { message = "Registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                dto.Password,
                lockoutOnFailure: true
            );

            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var accessToken = await _tokenService.GenerateAccessToken(user, _userManager);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var roles = await _userManager.GetRolesAsync(user);

            _db.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            await _db.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30), // Match your JWT expiry
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Roles= roles.ToList()
                }
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return Unauthorized();

            var storedToken = await _db.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
                return Unauthorized();

            var newAccessToken = await _tokenService.GenerateAccessToken(storedToken.User, _userManager);
            return Ok(new AuthResponseDto { AccessToken = newAccessToken });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (stored != null)
            {
                stored.IsRevoked = true;
                await _db.SaveChangesAsync();
            }
            Response.Cookies.Delete("refreshToken");
            return Ok(new { message = "Logged out" });
        }


        [HttpPost("change-password")]
        [Authorize]
        
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            // get user Id
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }
            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPasword);
            if(result.Succeeded)
            {
                return Ok(new { message = "Password changed successfully" });
            }
            else
            {
                return BadRequest(result.Errors);
            }


        }
        [HttpPost("send-reset-code")]
        public async Task<IActionResult> SendResetCode([FromBody] SendCodeRequestDTO request)
        {
            // 1. Locate the user in your database
             var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
             if (user == null) return NotFound("User not found.");

            // 2. Generate a secure, 6-digit numeric string
            string sixDigitCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            // 3. Save code & short lifespan (e.g., 15 minutes) to the user record
             user.ResetCode = sixDigitCode;
             user.ResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);
             await _db.SaveChangesAsync();

            // 4. Construct a professional HTML verification template
            string htmlMessage = $@"
            <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                <h3 style='color: #2D3748; margin-bottom: 5px;'>Eduverse Account Recovery</h3>
                <p style='color: #4A5568; font-size: 14px;'>Use the verification code below to complete your password reset request. This code is active for 15 minutes.</p>
                <div style='background-color: #EDF2F7; padding: 15px; border-radius: 6px; font-size: 28px; font-weight: bold; letter-spacing: 4px; text-align: center; color: #2B6CB0; margin: 20px 0;'>
                    {sixDigitCode}
                </div>
                <p style='color: #718096; font-size: 12px;'>If you did not request this change, please safely disregard this notice.</p>
            </div>";

            try
            {
                await _emailService.SendEmailAsync(request.Email, "Your Eduverse Reset Code", htmlMessage);
                return Ok("Verification code sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"SMTP Dispatch Error: {ex.Message}");
            }
        }

        [HttpPost("verify-and-reset")]
        public async Task<IActionResult> VerifyAndReset([FromBody] VerifyAndResetRequestDTO request)
        {
            // 1. Fetch user by email using UserManager
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return NotFound("User not found.");

            // 2. Validate your custom 6-digit code matches and hasn't expired
            if (user.ResetCode != request.VerificationCode || user.ResetCodeExpiry < DateTime.UtcNow)
            {
                return BadRequest("The code is incorrect or has expired.");
            }

            // 3. Generate an internal Identity token to satisfy the ResetPasswordAsync signature
            var identityResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // 4. Reset the password (this automatically validates strength, hashes it, and saves it)
            var result = await _userManager.ResetPasswordAsync(user, identityResetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                // Return errors if the new password fails your identity rules (e.g., too short, missing uppercase)
                return BadRequest(result.Errors);
            }

            // 5. Clean up your temporary custom code fields
            user.ResetCode = string.Empty;
            user.ResetCodeExpiry = null;
            await _userManager.UpdateAsync(user); // Save the cleared code fields

            return Ok("Your password has been changed successfully. You can now log in.");
        }
    }

}
