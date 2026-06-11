namespace E7jezli.Server.Models
{
    public class Business
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = "رام الله"; // Fixed to Ramallah only
        public string Category { get; set; } = string.Empty;
        
        // Service Type: determines booking rules
        // Options: "wedding_hall", "restaurant", "coffee_shop", "gym", "sports_field", 
        //          "entertainment_center", "delivery_company", "hotel", "beauty_clinic", 
        //          "womens_salon", "mens_salon"
        public string ServiceType { get; set; } = string.Empty;
        
        // Capacity: maximum number of people/clients per time slot
        public int Capacity { get; set; } = 1;
        
        // Business credentials for login
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        
        // Contact information
        public string PhoneNumber { get; set; } = string.Empty;
        
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
        
        // Status: "active" only (removed subscription system)
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
