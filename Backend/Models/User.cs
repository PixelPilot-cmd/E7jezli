using System;

namespace E7jezli.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int Points { get; set; } = 150; // 150 points for signup welcome
        public bool IsPartner { get; set; } = false; // indicates if user is a business partner
        public int? BusinessId { get; set; } // optional link to partner business
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
