using GemStonesApi.Interfaces;
using GemStonesApi.Models;
using GemStonesApi.ViewModels;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GemStonesApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repository;
        private readonly IConfiguration _config;

        public AuthService(
            IAuthRepository repository,
            IConfiguration config)
        {
            _repository = repository;
            _config = config;
        }

        public async Task<AuthResponseVM> RegisterAsync(RegisterVM viewModel)
        {
            // Hash the password — never store plain text
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(
                viewModel.Password, workFactor: 12);

            var user = new User
            {
                Username = viewModel.Username.Trim().ToLower(),
                Email = viewModel.Email.Trim().ToLower(),
                PasswordHash = passwordHash,
                Role = "User"
            };

            await _repository.RegisterUserAsync(user);

            // After register, log them in immediately
            var token = GenerateJwtToken(user);
            var expiry = DateTime.UtcNow.AddHours(
                double.Parse(_config["Jwt:ExpiryInHours"]));

            return new AuthResponseVM
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = expiry
            };
        }

        public async Task<AuthResponseVM> LoginAsync(LoginVM viewModel)
        {
            // Find user by username
            var user = await _repository
                .GetUserByUsernameAsync(
                    viewModel.Username.Trim().ToLower());

            // User not found
            if (user == null)
                throw new UnauthorizedAccessException(
                    "Invalid username or password");

            // Verify password against stored hash
            var isPasswordValid = BCrypt.Net.BCrypt
                .Verify(viewModel.Password, user.PasswordHash);

            if (!isPasswordValid)
                throw new UnauthorizedAccessException(
                    "Invalid username or password");

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var expiry = DateTime.UtcNow.AddHours(
                double.Parse(_config["Jwt:ExpiryInHours"]));

            return new AuthResponseVM
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = expiry
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            // Claims are the data embedded inside the token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,
                    user.Id.ToString()),
                new Claim(ClaimTypes.Name,
                    user.Username),
                new Claim(ClaimTypes.Email,
                    user.Email),
                new Claim(ClaimTypes.Role,
                    user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(
                    double.Parse(_config["Jwt:ExpiryInHours"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}