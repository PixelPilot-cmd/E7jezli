namespace E7jezli.Server.Models
{
    public class Business
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        
        // لدعم رفع الصور (يمكن أن يكون رابط أو Base64)
        public string ImageUrl { get; set; } = string.Empty;
        
        // روابط التواصل الاجتماعي (اختيارية)
        public string? FacebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? WhatsappLink { get; set; }
        
        public string? Description { get; set; }
        public string? SecondaryImages { get; set; } // Comma-separated list of additional images
        public string? ExtraServices { get; set; } // JSON or text list of extra service categories/details
        
        public double Rating { get; set; } = 5.0;
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
