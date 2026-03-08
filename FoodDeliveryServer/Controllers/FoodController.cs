using FoodDeliveryServer.Dtos;
using FoodDeliveryServer.Models;
using FoodDeliveryServer.Services; // 👈 Remember to reference this!
using FoodDeliveryServer.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodController : ControllerBase
    {
        // 1. Previously this was _context, now it is _foodService
        private readonly IFoodService _foodService;

        // 2. Constructor: Ask the system for someone who "knows the IFoodService standard"
        public FoodController(IFoodService foodService)
        {
            _foodService = foodService;
        }

        // GET: api/Food
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FoodItem>>> GetFoodItems()
        {
            // 3. No longer query the database manually, instead instruct the Service directly
            var foods = await _foodService.GetAllFoods();
            return Ok(foods);
        }

        [Authorize(Roles = Constants.Roles.Admin)]
        // POST: api/Food
        [HttpPost]
        public async Task<ActionResult<FoodItem>> PostFoodItem([FromForm] FoodCreateDto request)
        {
            var foodItem = new FoodItem
            {
                Name = request.Name,
                Price = request.Price
            };
            
            if(request.Image != null)
            {
                // a. Generate a unique file name (e.g., pizza_GUID.jpg) to prevent name collision and overwriting
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.Image.FileName);
                // b. Construct the absolute path to save to disk (Your computer/wwwroot/images/xxx.jpg)
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                // c. Create file stream, and save the image
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream);
                }

                // d. Save the "Network Path" to the database (Note: save URL, not the file itself)
                // e.g.: /images/xxx.jpg
                foodItem.ImageUrl = "/images/" + fileName;
            }

            var createdFoodItem = await _foodService.AddFood(foodItem);

            return Ok(createdFoodItem);
        }

        [Authorize(Roles = Constants.Roles.Admin)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFoodItem(int id)
        {
            await _foodService.DeleteFood(id);
            return NoContent();
        }


        [HttpGet("error-test")] // URL: GET /api/Food/error-test
        public IActionResult GenerateError()
        {
            // Intentionally throw an exception
            throw new Exception("This is a deliberate explosion! 💥");
        }
    }
}