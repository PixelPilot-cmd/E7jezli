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
    public class BusinessesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BusinessesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Businesses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Business>>> GetBusinesses()
        {
            return await _context.Businesses.OrderByDescending(b => b.CreatedAt).ToListAsync();
        }

        // GET: api/Businesses/{id}/credentials
        [HttpGet("{id}/credentials")]
        public async Task<ActionResult<object>> GetBusinessCredentials(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound("المؤسسة غير موجودة.");
            }
            return Ok(new { 
                Username = business.Username, 
                Password = business.PasswordHash,
                Note = "كلمة المرور مشفرة بـ SHA-256"
            });
        }

        // GET: api/Businesses/{id}
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

        // POST: api/Businesses (Admin adds business with credentials)
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

        // PUT: api/Businesses/{id} (Update business)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBusiness(int id, [FromBody] UpdateBusinessDto dto)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(dto.Name)) business.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Category)) business.Category = dto.Category;
            if (!string.IsNullOrEmpty(dto.ServiceType)) business.ServiceType = dto.ServiceType;
            if (dto.Capacity.HasValue) business.Capacity = dto.Capacity.Value;
            if (!string.IsNullOrEmpty(dto.Username)) business.Username = dto.Username;
            if (!string.IsNullOrEmpty(dto.Password)) business.PasswordHash = HashPassword(dto.Password);
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) business.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrEmpty(dto.ImageUrl)) business.ImageUrl = dto.ImageUrl;
            if (!string.IsNullOrEmpty(dto.FacebookLink)) business.FacebookLink = dto.FacebookLink;
            if (!string.IsNullOrEmpty(dto.InstagramLink)) business.InstagramLink = dto.InstagramLink;
            if (!string.IsNullOrEmpty(dto.WhatsappLink)) business.WhatsappLink = dto.WhatsappLink;
            if (!string.IsNullOrEmpty(dto.Description)) business.Description = dto.Description;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Businesses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBusiness(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null) return NotFound();

            _context.Businesses.Remove(business);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/Businesses/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBusinessStatus(int id, [FromBody] string newStatus)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null) return NotFound();

            business.Status = newStatus;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

    public class CreateBusinessDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string FacebookLink { get; set; } = string.Empty;
        public string InstagramLink { get; set; } = string.Empty;
        public string WhatsappLink { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SecondaryImages { get; set; } = string.Empty;
        public string ExtraServices { get; set; } = string.Empty;
    }

    public class UpdateBusinessDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public int? Capacity { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string FacebookLink { get; set; } = string.Empty;
        public string InstagramLink { get; set; } = string.Empty;
        public string WhatsappLink { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
