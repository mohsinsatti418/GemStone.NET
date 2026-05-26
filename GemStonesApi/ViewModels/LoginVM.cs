using System.ComponentModel.DataAnnotations;

namespace GemStonesApi.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Captcha token is required")]
        public string CaptchaToken { get; set; }
    }
}