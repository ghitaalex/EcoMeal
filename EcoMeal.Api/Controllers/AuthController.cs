using EcoMeal.Api.Constants;
using EcoMeal.Api.Entities;
using EcoMeal.Api.Models.Auth;
using EcoMeal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EcoMeal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<User> userManager,
            EmailService emailService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                Name = request.Name,
                Contact = request.Contact
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, UserRoles.User);

            try
            {
                var body = await _emailService.LoadTemplateAsync("Welcome", new Dictionary<string, string>
                    {
                        { "UserName", user.Name }
                    });

                await _emailService.SendEmailAsync(user.Email, user.Name, "Welcome to EcoMeal! 🌱", body);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to send welcome email to user {UserId}",
                    user.Id);
            }

            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPost("login-notification")]
        [Authorize]
        public async Task<IActionResult> SendLoginNotification()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning(
                    "Cannot send login notification because user {UserId} has no email address",
                    user.Id);
                return NoContent();
            }

            try
            {
                var userName = string.IsNullOrWhiteSpace(user.Name) ? "there" : user.Name;
                var body = await _emailService.LoadTemplateAsync(
                    "LoginNotification",
                    new Dictionary<string, string>
                    {
                        { "UserName", userName },
                        { "LoginTime", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'") }
                    });

                await _emailService.SendEmailAsync(
                    user.Email,
                    userName,
                    "New login to your EcoMeal account",
                    body);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to send login notification to user {UserId}",
                    user.Id);
            }

            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new UserMeResponse
            {
                Email = user.Email,
                Name = user.Name,
                Contact = user.Contact,
                Roles = roles
            });
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            user.Name = request.Name;
            user.Contact = request.Contact;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { Message = "Profile updated successfully" });
        }
    }
}
