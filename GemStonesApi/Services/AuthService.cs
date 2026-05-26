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
        private readonly ICaptchaService _captcha;

        public AuthService(
            IAuthRepository repository,
            IConfiguration config,
            ICaptchaService captcha)
        {
            _repository = repository;
            _config = config;
            _captcha = captcha;
        }

        public async Task<AuthResponseVM> RegisterAsync(
              RegisterVM viewModel)
        {
            // Validate captcha first
            var isHuman = await _captcha
                .ValidateAsync(viewModel.CaptchaToken);

            if (!isHuman)
                throw new UnauthorizedAccessException(
                    "Captcha validation failed.");

            // rest of register code unchanged...
            var passwordHash = BCrypt.Net.BCrypt
                .HashPassword(viewModel.Password, workFactor: 12);

            var user = new User
            {
                Username = viewModel.Username.Trim().ToLower(),
                Email = viewModel.Email.Trim().ToLower(),
                PasswordHash = passwordHash,
                Role = "User"
            };

            await _repository.RegisterUserAsync(user);

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
            // 1. Captcha check
            var isHuman = await _captcha
                .ValidateAsync(viewModel.CaptchaToken);
            if (!isHuman)
                throw new UnauthorizedAccessException(
                    "Captcha validation failed.");

            // 2. Find user
            var user = await _repository
                .GetUserByUsernameAsync(
                    viewModel.Username.Trim().ToLower());

            // 3. Same error for wrong username OR wrong password
            //    prevents username enumeration
            if (user == null)
                throw new UnauthorizedAccessException(
                    "Invalid username or password");

            // 4. Check if account is locked
            if (user.LockoutUntil.HasValue
                && user.LockoutUntil.Value > DateTime.UtcNow)
            {
                var minutesLeft = (int)Math.Ceiling(
                    (user.LockoutUntil.Value - DateTime.UtcNow)
                    .TotalMinutes);

                throw new UnauthorizedAccessException(
                    $"Account locked. Try again in {minutesLeft} minutes.");
            }

            // 5. Verify password
            var isPasswordValid = BCrypt.Net.BCrypt
                .Verify(viewModel.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                // Increment failed attempts
                var attempts = user.FailedLoginAttempts + 1;
                DateTime? lockoutUntil = null;

                // Lock after 5 failed attempts for 30 minutes
                if (attempts >= 5)
                {
                    lockoutUntil = DateTime.UtcNow
                        .AddMinutes(30);
                }

                await _repository.UpdateLoginAttemptsAsync(
                    user.Username, attempts, lockoutUntil);

                // Tell user how many attempts remain
                var remaining = 5 - attempts;
                if (remaining > 0)
                    throw new UnauthorizedAccessException(
                        $"Invalid username or password. " +
                        $"{remaining} attempts remaining.");
                else
                    throw new UnauthorizedAccessException(
                        "Account locked for 30 minutes due to " +
                        "too many failed attempts.");
            }

            // 6. Successful login — reset failed attempts
            await _repository.ResetLoginAttemptsAsync(user.Username);

            // 7. Generate token
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