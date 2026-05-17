namespace GemStonesApi.Models
{
    public class GemStone
    {
        public int Id { get; set; }

        // Basic Identity
        public string Name { get; set; }
        public string SKU { get; set; }
        public string GemType { get; set; }

        // The 4 Cs - Industry standard grading
        public decimal WeightInCarats { get; set; }
        public string Color { get; set; }
        public string Clarity { get; set; }
        public string Cut { get; set; }

        // Physical Details
        public string Shape { get; set; }
        public decimal LengthMM { get; set; }
        public decimal WidthMM { get; set; }
        public decimal DepthMM { get; set; }

        // Origin & Sourcing
        public string Origin { get; set; }
        public string Treatment { get; set; }
        public bool IsNatural { get; set; }

        // Certification
        public string CertificationLab { get; set; }
        public string CertificateNumber { get; set; }

        // Pricing & Inventory
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }

        // Description
        public string Description { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}