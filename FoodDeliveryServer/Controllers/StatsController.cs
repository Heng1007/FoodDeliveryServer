using FoodDeliveryServer.Data;
using FoodDeliveryServer.Models;
using FoodDeliveryServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private IStatServices _statServices;

        public StatsController(IStatServices service)
        {
            _statServices = service;
        }

        // GET: api/Stats/BestSellers
        // Get the top 3 best-selling dishes from the past 7 days
        [HttpGet("BestSellers")]
        public async Task<ActionResult> GetBestSellers()
        {
            var bestSellers = await _statServices.GetBestSellers();
            return Ok(bestSellers);
        }

        [HttpGet("TopSpender")]
        public async Task<ActionResult> GetTopSpender()
        {
            var topSpender = await _statServices.GetTopSpender();
            return Ok(topSpender);
        }

        [HttpGet("Sentiment")]
        public async Task<ActionResult> GetSentimentStats()
        {
            var sentimentStats = await _statServices.GetSentimentStats();
            return Ok(sentimentStats);
        }
    }
}