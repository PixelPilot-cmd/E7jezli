using System;

namespace E7jezli.Server.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessImage { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        
        // Time slot management for conflict detection
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        // Number of people for capacity checking
        public int NumberOfPeople { get; set; } = 1;
        
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserPhoneNumber { get; set; } = string.Empty;
        
        // Status: "pending", "confirmed", "completed", "cancelled"
        public string Status { get; set; } = "pending";
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
