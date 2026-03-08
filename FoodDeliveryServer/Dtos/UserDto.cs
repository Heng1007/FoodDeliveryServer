using System.ComponentModel.DataAnnotations;

namespace FoodDeliveryServer.Dtos
{
    public class UserDto
    {
        // Required: Username
        [Required(ErrorMessage = "Username must be filled!")]
        public string Username { get; set; } = string.Empty;

        // Required: Original password (e.g., "123456")
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;
    }
}
