using System;

namespace E7jezli.Server.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public int UserId { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public string Service { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled
    }
}
