using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E7jezli.Server.Data;
using E7jezli.Server.Models;
using System.Security.Cryptography;
using System.Text;

namespace E7jezli.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.FullName))
            {
                return BadRequest("جميع الحقول مطلوبة.");
            }

            var emailNormalized = dto.Email.Trim().ToLower();
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == emailNormalized))
            {
                return BadRequest("البريد الإلكتروني مسجل بالفعل.");
            }

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = emailNormalized,
                PasswordHash = HashPassword(dto.Password),
                Points = 150, // 150 points welcome gift
                DateCreated = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("البريد الإلكتروني وكلمة المرور مطلوبة.");
            }

            var emailNormalized = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalized);

            if (user == null || user.PasswordHash != HashPassword(dto.Password))
            {
                return Unauthorized("البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            }

            return Ok(user);
        }

        // GET: api/Auth/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("البريد الإلكتروني مطلوب.");
            }

            var emailNormalized = email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalized);
            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            return Ok(user);
        }

        // POST: api/Auth/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest("جميع الحقول مطلوبة.");
            }

            var emailNormalized = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalized);
            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            if (user.PasswordHash != HashPassword(dto.OldPassword))
            {
                return BadRequest("كلمة المرور القديمة غير صحيحة.");
            }

            user.PasswordHash = HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تغيير كلمة المرور بنجاح." });
        }

        // POST: api/Auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest("جميع الحقول مطلوبة.");
            }

            var emailNormalized = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalized);
            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            user.PasswordHash = HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تمت إعادة تعيين كلمة المرور بنجاح." });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }

    public class RegisterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
