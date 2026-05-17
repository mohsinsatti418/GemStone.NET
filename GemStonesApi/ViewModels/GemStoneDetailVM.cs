namespace GemStonesApi.ViewModels
{
    public class GemStoneDetailVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string GemType { get; set; }

        // 4 Cs
        public decimal WeightInCarats { get; set; }
        public string Color { get; set; }
        public string Clarity { get; set; }
        public string Cut { get; set; }

        // Physical
        public string Shape { get; set; }
        public decimal LengthMM { get; set; }
        public decimal WidthMM { get; set; }
        public decimal DepthMM { get; set; }

        // Origin
        public string Origin { get; set; }
        public string Treatment { get; set; }
        public bool IsNatural { get; set; }

        // Certification
        public string CertificationLab { get; set; }
        public string CertificateNumber { get; set; }

        // Pricing
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }

        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // All images — this is the key difference from GemStoneListVM
        public List<GemStoneImageVM> Images { get; set; }
                                     = new List<GemStoneImageVM>();
    }
}