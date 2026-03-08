using FoodDeliveryServer.Dtos;
using System.Net;

namespace FoodDeliveryServer.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next; // Next handler in the pipeline
        private readonly ILogger<ExceptionMiddleware> _logger; // Logger for recording errors

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // 👇 Core logic: intercepts all requests
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // 1. Attempt to let it pass through to the Controller
                await _next(context);
            }
            catch (Exception ex)
            {
                // 2. If the Controller throws an error, it gets caught here!
                _logger.LogError(ex, $"Something went wrong: {ex.Message}"); // Log it for developers to see

                // 3. Handle the error and return an elegant JSON response to the user
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // 👇 This switch statement is the core! It determines the status code based on the exception type
            switch (exception)
            {
                // If "Unauthorized" exception (wrong password) -> return 401 Unauthorized
                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                // If "Invalid Arguments" exception (e.g., out of stock) -> return 400 Bad Request
                // You can use this later: throw new ArgumentException("Out of stock");
                case ArgumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                // All other unknown errors -> return 500 Internal Server Error
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            // Build error response
            var response = new ErrorResponse
            {
                StatusCode = context.Response.StatusCode,
                // If it's a 500, for security, do not expose exception.Message to the user, return "Internal Server Error"
                // If it's 401/400, show the Message to the user ("Username or password incorrect.")
                Message = context.Response.StatusCode == 500 ? "Internal Server Error" : exception.Message
            };

            await context.Response.WriteAsync(response.ToString());
        }
    }
}
