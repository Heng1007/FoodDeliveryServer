using FoodDeliveryServer.Dtos;
using FoodDeliveryServer.Models;
using FoodDeliveryServer.Services;
using FoodDeliveryServer.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDeliveryServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        private int CurrentUserId
        {
            get
            {
                var idClaim = User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int userId))
                {
                    // If ID cannot be retrieved, throw an exception or return 0 (depending on context)
                    // For simplicity, if not found, it usually means [Authorize] has not taken effect or Token is invalid
                    return 0;
                }
                return userId;
            }
        }

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // 1. Get all orders
        [Authorize(Roles = Constants.Roles.Admin)]
        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetOrders()
        {
            var order = await _orderService.GetOrders();
            return Ok(order);
        }

        [Authorize]
        [HttpGet("MyOrders")]
        public async Task<ActionResult<List<Order>>> GetMyOrders()
        {
            if (CurrentUserId == 0)
            {
                return Unauthorized("User identity could not be verified");
            }
            var orders = await _orderService.GetOrdersByUserId(CurrentUserId);
            return Ok(orders);
        }


        // 2. Place order (Create Order)
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto dto)
        {
            if (CurrentUserId == 0)
            {
                return Unauthorized("User identity could not be verified");
            }
            var order = await _orderService.CreateOrder(CurrentUserId, dto);

            return Ok(order);
        }

        [HttpGet("TopSpender")]
        public async Task<ActionResult<TopSpenderDto>> GetTopSpender()
        {
            var result = await _orderService.GetTopSpender();
            if (result == null)
            {
                return NotFound("Currently no orders");
            }
            return Ok(result);
        }


        [Authorize(Roles = Constants.Roles.Admin)]
        [HttpGet("PagedResult")]
        // 👇 [FromQuery] means the parameter comes from the URL query string (e.g., ?page=2)
        public async Task<ActionResult<PagedResult<Order>>> GetOrders(
            [FromQuery] int page = 1,      // Default to page 1
            [FromQuery] int pageSize = 10  // Default to 10 items per page
        )
        {
            // Defensive programming: Prevent passing page=-1 or pageSize=1000000
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var result = await _orderService.GetOrdersAsync(page, pageSize);
            return Ok(result);
        }

        [Authorize(Roles = Constants.Roles.Admin)]
        [HttpPatch("{orderId}/Status")]
        public async Task<ActionResult> SetOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto dto)
        {
            await _orderService.SetOrderStatus(orderId, dto.Status);
            return NoContent();
        }

        [Authorize]
        [HttpPost("{orderId}/Cancel")]
        public async Task<ActionResult<string>> CancelOrder(int orderId)
        {
            if (CurrentUserId == 0)
            {
                return Unauthorized("User identity could not be verified");
            }

            var errorMsg = await _orderService.CancelOrder(orderId, CurrentUserId);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                return BadRequest(errorMsg);
            }

            return Ok("Order " + orderId + " has been successfully cancelled!");
        }

        [Authorize]
        [HttpGet("TotalRevenue")]
        public async Task<ActionResult<decimal>> GetTotalRevenue()
        {
            var totalRevenue = await _orderService.GetTotalRevenue();
            return Ok(totalRevenue);
        }
    }
}