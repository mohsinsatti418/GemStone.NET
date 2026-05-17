using GemStonesApi.Models;
using GemStonesApi.ViewModels;

namespace GemStonesApi.Maps
{
    public static class GemStoneMap
    {
        public static GemStone ToModel(GemStoneCreateVM vm)
        {
            return new GemStone
            {
                Name = vm.Name,
                SKU = vm.SKU,
                GemType = vm.GemType,
                WeightInCarats = vm.WeightInCarats,
                Color = vm.Color,
                Clarity = vm.Clarity,
                Cut = vm.Cut,
                Shape = vm.Shape,
                LengthMM = vm.LengthMM,
                WidthMM = vm.WidthMM,
                DepthMM = vm.DepthMM,
                Origin = vm.Origin,
                Treatment = vm.Treatment,
                IsNatural = vm.IsNatural,
                CertificationLab = vm.CertificationLab,
                CertificateNumber = vm.CertificateNumber,
                Price = vm.Price,
                StockQuantity = vm.StockQuantity,
                IsAvailable = true,
                Description = vm.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static GemStone ToModel(GemStoneUpdateVM vm)
        {
            return new GemStone
            {
                Id = vm.Id,
                Name = vm.Name,
                SKU = vm.SKU,
                GemType = vm.GemType,
                WeightInCarats = vm.WeightInCarats,
                Color = vm.Color,
                Clarity = vm.Clarity,
                Cut = vm.Cut,
                Shape = vm.Shape,
                LengthMM = vm.LengthMM,
                WidthMM = vm.WidthMM,
                DepthMM = vm.DepthMM,
                Origin = vm.Origin,
                Treatment = vm.Treatment,
                IsNatural = vm.IsNatural,
                CertificationLab = vm.CertificationLab,
                CertificateNumber = vm.CertificateNumber,
                Price = vm.Price,
                StockQuantity = vm.StockQuantity,
                IsAvailable = vm.IsAvailable,
                Description = vm.Description,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}