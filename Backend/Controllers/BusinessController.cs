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
    public class BusinessController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BusinessController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Business
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Business>>> GetBusinesses()
        {
            return await _context.Businesses.OrderByDescending(b => b.CreatedAt).ToListAsync();
        }

        // POST: api/Business (Admin adds business with credentials)
        [HttpPost]
        public async Task<ActionResult<Business>> PostBusiness([FromBody] CreateBusinessDto dto)
        {
            // Check if username already exists
            if (await _context.Businesses.AnyAsync(b => b.Username == dto.Username))
            {
                return BadRequest("اسم المستخدم موجود بالفعل.");
            }

            var business = new Business
            {
                Name = dto.Name,
                Location = "رام الله", // Fixed to Ramallah
                Category = dto.Category,
                ServiceType = dto.ServiceType,
                Capacity = dto.Capacity,
                Username = dto.Username,
                PasswordHash = HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber,
                ImageUrl = dto.ImageUrl,
                FacebookLink = dto.FacebookLink,
                InstagramLink = dto.InstagramLink,
                WhatsappLink = dto.WhatsappLink,
                Description = dto.Description,
                SecondaryImages = dto.SecondaryImages,
                ExtraServices = dto.ExtraServices,
                Rating = 5.0,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Businesses.Add(business);
            await _context.SaveChangesAsync();

            return Ok(business);
        }

        // DELETE: api/Business/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBusiness(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            _context.Businesses.Remove(business);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Business/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateBusinessStatus(int id, [FromBody] string status)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            business.Status = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Business/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Business>> GetBusiness(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound("المؤسسة غير موجودة.");
            }
            return Ok(business);
        }

        // PUT: api/Business/{id} (Update business)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBusiness(int id, [FromBody] UpdateBusinessDto dto)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            business.Name = dto.Name ?? business.Name;
            business.Category = dto.Category ?? business.Category;
            business.ServiceType = dto.ServiceType ?? business.ServiceType;
            business.Capacity = dto.Capacity > 0 ? dto.Capacity : business.Capacity;
            business.PhoneNumber = dto.PhoneNumber ?? business.PhoneNumber;
            business.ImageUrl = dto.ImageUrl ?? business.ImageUrl;
            business.FacebookLink = dto.FacebookLink ?? business.FacebookLink;
            business.InstagramLink = dto.InstagramLink ?? business.InstagramLink;
            business.WhatsappLink = dto.WhatsappLink ?? business.WhatsappLink;
            business.Description = dto.Description ?? business.Description;
            business.SecondaryImages = dto.SecondaryImages ?? business.SecondaryImages;
            business.ExtraServices = dto.ExtraServices ?? business.ExtraServices;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                business.PasswordHash = HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();
            return NoContent();
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

    public class CreateBusinessDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public int Capacity { get; set; } = 1;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? FacebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? WhatsappLink { get; set; }
        public string? Description { get; set; }
        public string? SecondaryImages { get; set; }
        public string? ExtraServices { get; set; }
    }

    public class UpdateBusinessDto
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? ServiceType { get; set; }
        public int Capacity { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public string? FacebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? WhatsappLink { get; set; }
        public string? Description { get; set; }
        public string? SecondaryImages { get; set; }
        public string? ExtraServices { get; set; }
        public string? Password { get; set; }
    }
}
