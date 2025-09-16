using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Please input a valid email!")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Please input password!")]
        public string? Password { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Please input a valid email!")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Please input password!")]
        public string? Password { get; set; }
    }
}