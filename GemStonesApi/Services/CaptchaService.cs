using GemStonesApi.Interfaces;
using System.Text.Json;

namespace GemStonesApi.Services
{
    public class CaptchaService : ICaptchaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public CaptchaService(
            IHttpClientFactory httpClientFactory,
            IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<bool> ValidateAsync(string token)
        {
            var secretKey = _config["ReCaptcha:SecretKey"];
            var minScore = double.Parse(
                _config["ReCaptcha:MinimumScore"] ?? "0.5");

            var client = _httpClientFactory.CreateClient();

            // Call Google's verification endpoint
            var response = await client.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "secret",   secretKey },
                    { "response", token     }
                })
            );

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<
                CaptchaResponse > (json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            // Must be successful AND score above threshold
            return result?.Success == true
                && result.Score >= minScore;
        }

        // Maps Google's JSON response
        private class CaptchaResponse
        {
            public bool Success { get; set; }
            public double Score { get; set; }
            public string Action { get; set; }
            public string Hostname { get; set; }
        }
    }
}