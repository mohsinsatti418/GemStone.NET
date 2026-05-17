namespace GemStonesApi.ViewModels
{
    public class GemStoneListVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string GemType { get; set; }
        public decimal WeightInCarats { get; set; }
        public string Color { get; set; }
        public string Clarity { get; set; }
        public string Cut { get; set; }
        public string Shape { get; set; }
        public string Origin { get; set; }
        public bool IsNatural { get; set; }
        public string CertificationLab { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }   // only thumbnail, not all images
        public DateTime CreatedAt { get; set; }
    }
}