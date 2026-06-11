using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E7jezli.Server.Data;
using E7jezli.Server.Models;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace E7jezli.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private string GenerateJwtToken(int userId, string email, string role = "user")
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyForE7jezliRamallah2026!@#";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "E7jezliRamallah";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "E7jezliUsers";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
                PhoneNumber = dto.PhoneNumber?.Trim() ?? string.Empty,
                DateCreated = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user.Id, user.Email);
            return Ok(new { user.Id, user.FullName, user.Email, user.PhoneNumber, Token = token });
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

            var token = GenerateJwtToken(user.Id, user.Email);
            return Ok(new { user.Id, user.FullName, user.Email, user.PhoneNumber, Token = token });
        }

        // POST: api/Auth/business-login
        [HttpPost("business-login")]
        public async Task<IActionResult> BusinessLogin([FromBody] BusinessLoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("اسم المستخدم وكلمة المرور مطلوبة.");
            }

            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Username == dto.Username);

            if (business == null || business.PasswordHash != HashPassword(dto.Password))
            {
                return Unauthorized("اسم المستخدم أو كلمة المرور غير صحيحة.");
            }

            var token = GenerateJwtToken(business.Id, business.Username, "business");
            return Ok(new { business.Id, business.Name, business.Username, business.Category, business.ServiceType, business.Capacity, Token = token });
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

        // POST: api/Auth/admin-login
        [HttpPost("admin-login")]
        public IActionResult AdminLogin([FromBody] AdminLoginDto dto)
        {
            var adminPassword = _configuration["AdminPassword"] ?? "Ramallah@2026!AdminSecure";
            
            if (dto.Password != adminPassword)
            {
                return Unauthorized("كلمة المرور غير صحيحة.");
            }

            var token = GenerateJwtToken(0, "admin@e7jezli-ramallah.com", "admin");
            return Ok(new { Token = token, Message = "تم تسجيل الدخول بنجاح" });
        }
    }

    public class AdminLoginDto
    {
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class BusinessLoginDto
    {
        public string Username { get; set; } = string.Empty;
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
