using System.ComponentModel.DataAnnotations;

namespace GemStonesApi.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Username is required")]
        [MaxLength(100)]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(200)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [MaxLength(100)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Captcha token is required")]
        public string CaptchaToken { get; set; }
    }
}