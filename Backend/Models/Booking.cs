using System;

namespace E7jezli.Server.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Time { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string BusinessImage { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public string Status { get; set; } = "pending";
    }
}
