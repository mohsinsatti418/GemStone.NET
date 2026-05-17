using Dapper;
using GemStonesApi.Interfaces;
using GemStonesApi.Models;
using GemStonesApi.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GemStonesApi.Repositories
{
    public class GemStoneRepository : IGemStoneRepository
    {
        private readonly string _connectionString;

        public GemStoneRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateGemStoneAsync(GemStone gemStone)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Name", gemStone.Name);
            parameters.Add("@SKU", gemStone.SKU);
            parameters.Add("@GemType", gemStone.GemType);
            parameters.Add("@WeightInCarats", gemStone.WeightInCarats);
            parameters.Add("@Color", gemStone.Color);
            parameters.Add("@Clarity", gemStone.Clarity);
            parameters.Add("@Cut", gemStone.Cut);
            parameters.Add("@Shape", gemStone.Shape);
            parameters.Add("@LengthMM", gemStone.LengthMM);
            parameters.Add("@WidthMM", gemStone.WidthMM);
            parameters.Add("@DepthMM", gemStone.DepthMM);
            parameters.Add("@Origin", gemStone.Origin);
            parameters.Add("@Treatment", gemStone.Treatment);
            parameters.Add("@IsNatural", gemStone.IsNatural);
            parameters.Add("@CertificationLab", gemStone.CertificationLab);
            parameters.Add("@CertificateNumber", gemStone.CertificateNumber);
            parameters.Add("@Price", gemStone.Price);
            parameters.Add("@StockQuantity", gemStone.StockQuantity);
            parameters.Add("@IsAvailable", gemStone.IsAvailable);
            parameters.Add("@Description", gemStone.Description);
            parameters.Add("@CreatedAt", gemStone.CreatedAt);
            parameters.Add("@UpdatedAt", gemStone.UpdatedAt);

            var newId = await connection.ExecuteScalarAsync<int>(
                "sp_CreateGemStone",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return newId;
        }

        public async Task CreateGemStoneImageAsync(GemStoneImage image)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@GemStoneId", image.GemStoneId);
            parameters.Add("@ImageUrl", image.ImageUrl);
            parameters.Add("@ImageName", image.ImageName);
            parameters.Add("@IsThumbnail", image.IsThumbnail);
            parameters.Add("@DisplayOrder", image.DisplayOrder);
            parameters.Add("@UploadedAt", image.UploadedAt);

            await connection.ExecuteAsync(
                "sp_CreateGemStoneImage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task SetThumbnailAsync(int gemStoneId, int imageId)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@GemStoneId", gemStoneId);
            parameters.Add("@ImageId", imageId);

            await connection.ExecuteAsync(
                "sp_SetGemStoneThumbnail",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<GemStoneListVM>> GetAllGemStonesAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<GemStoneListVM>(
                "sp_GetAllGemStones",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<GemStoneDetailVM> GetGemStoneByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            // QueryMultipleAsync lets us read two result sets
            // from one stored procedure in one database call
            using var multi = await connection.QueryMultipleAsync(
                "sp_GetGemStoneById",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            // Read first result set — the stone details
            var gemStone = await multi.ReadSingleOrDefaultAsync<GemStoneDetailVM>();

            // If no stone found with this Id, return null immediately
            if (gemStone == null)
                return null;

            // Read second result set — all images for this stone
            var images = await multi.ReadAsync<GemStoneImageVM>();

            // Attach the images to the stone
            gemStone.Images = images.ToList();

            return gemStone;
        }

        public async Task UpdateGemStoneAsync(GemStone gemStone)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Id", gemStone.Id);
            parameters.Add("@Name", gemStone.Name);
            parameters.Add("@SKU", gemStone.SKU);
            parameters.Add("@GemType", gemStone.GemType);
            parameters.Add("@WeightInCarats", gemStone.WeightInCarats);
            parameters.Add("@Color", gemStone.Color);
            parameters.Add("@Clarity", gemStone.Clarity);
            parameters.Add("@Cut", gemStone.Cut);
            parameters.Add("@Shape", gemStone.Shape);
            parameters.Add("@LengthMM", gemStone.LengthMM);
            parameters.Add("@WidthMM", gemStone.WidthMM);
            parameters.Add("@DepthMM", gemStone.DepthMM);
            parameters.Add("@Origin", gemStone.Origin);
            parameters.Add("@Treatment", gemStone.Treatment);
            parameters.Add("@IsNatural", gemStone.IsNatural);
            parameters.Add("@CertificationLab", gemStone.CertificationLab);
            parameters.Add("@CertificateNumber", gemStone.CertificateNumber);
            parameters.Add("@Price", gemStone.Price);
            parameters.Add("@StockQuantity", gemStone.StockQuantity);
            parameters.Add("@IsAvailable", gemStone.IsAvailable);
            parameters.Add("@Description", gemStone.Description);
            parameters.Add("@UpdatedAt", gemStone.UpdatedAt);

            await connection.ExecuteAsync(
                "sp_UpdateGemStone",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task DeleteGemStoneImageAsync(int imageId, int gemStoneId)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@ImageId", imageId);
            parameters.Add("@GemStoneId", gemStoneId);

            await connection.ExecuteAsync(
                "sp_DeleteGemStoneImage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task DeleteGemStoneAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await connection.ExecuteAsync(
                "sp_DeleteGemStone",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task RestoreGemStoneAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await connection.ExecuteAsync(
                "sp_RestoreGemStone",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}