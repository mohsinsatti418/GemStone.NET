using System.ComponentModel.DataAnnotations;

namespace GemStonesApi.ViewModels
{
    public class GemStoneUpdateVM
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        [MaxLength(50)]
        public string SKU { get; set; }

        [Required(ErrorMessage = "Gem type is required")]
        [MaxLength(100)]
        public string GemType { get; set; }

        // 4 Cs
        [Required]
        [Range(0.01, 1000.00, ErrorMessage = "Weight must be between 0.01 and 1000 carats")]
        public decimal WeightInCarats { get; set; }

        [Required]
        [MaxLength(50)]
        public string Color { get; set; }

        [Required]
        [MaxLength(10)]
        public string Clarity { get; set; }

        [MaxLength(20)]
        public string Cut { get; set; }

        // Physical
        [Required]
        [MaxLength(50)]
        public string Shape { get; set; }

        [Range(0.01, 500)]
        public decimal LengthMM { get; set; }

        [Range(0.01, 500)]
        public decimal WidthMM { get; set; }

        [Range(0.01, 500)]
        public decimal DepthMM { get; set; }

        // Origin
        [MaxLength(100)]
        public string Origin { get; set; }

        [MaxLength(100)]
        public string Treatment { get; set; }

        public bool IsNatural { get; set; }

        // Certification
        [MaxLength(50)]
        public string CertificationLab { get; set; }

        [MaxLength(100)]
        public string CertificateNumber { get; set; }

        // Pricing
        [Required]
        [Range(0.01, 10000000)]
        public decimal Price { get; set; }

        [Range(0, 10000)]
        public int StockQuantity { get; set; }

        public bool IsAvailable { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        // Image management
        public List<int> DeleteImageIds { get; set; } = new List<int>();
        public int? NewThumbnailImageId { get; set; }
    }
}