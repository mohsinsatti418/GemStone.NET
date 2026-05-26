using Dapper;
using GemStonesApi.Interfaces;
using GemStonesApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GemStonesApi.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;

        public AuthRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> RegisterUserAsync(User user)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Username", user.Username);
            parameters.Add("@Email", user.Email);
            parameters.Add("@PasswordHash", user.PasswordHash);

            var newId = await connection.ExecuteScalarAsync<int>(
                "sp_RegisterUser",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return newId;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Username", username);

            return await connection.QuerySingleOrDefaultAsync<User>(
                "sp_GetUserByUsername",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task UpdateLoginAttemptsAsync(
    string username,
    int failedAttempts,
    DateTime? lockoutUntil)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Username", username);
            parameters.Add("@FailedAttempts", failedAttempts);
            parameters.Add("@LockoutUntil", lockoutUntil);

            await connection.ExecuteAsync(
                "sp_UpdateLoginAttempts",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task ResetLoginAttemptsAsync(string username)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Username", username);

            await connection.ExecuteAsync(
                "sp_ResetLoginAttempts",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}