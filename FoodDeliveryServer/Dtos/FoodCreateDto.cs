using System.ComponentModel.DataAnnotations;

namespace FoodDeliveryServer.Dtos
{
    public class FoodCreateDto
    {
        [Required(ErrorMessage = "Food name is required.")]
        [StringLength(100, ErrorMessage = "Food name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10,000.00.")]
        public decimal Price { get; set; }

        // 👇 IFormFile is specifically used to receive files uploaded from the frontend
        public IFormFile? Image { get; set; }
    }
}