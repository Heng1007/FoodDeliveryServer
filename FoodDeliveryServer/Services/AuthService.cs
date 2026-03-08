using FoodDeliveryServer.Data;
using FoodDeliveryServer.Dtos;
using FoodDeliveryServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FoodDeliveryServer.Utils;

namespace FoodDeliveryServer.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        public AuthService(AppDbContext appDbContext, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = appDbContext;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> Login(UserLoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                _logger.LogWarning($" Login Failed. Username '{request.Username}' does not exist");
                throw new UnauthorizedAccessException("Username does not exist.");
            }

            // 2. Verify password: Compare user's plain text input with the hash in the database
            // BCrypt automatically handles the Salt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning($"❌ Login Failed: Incorrect password for user '{request.Username}'");
                throw new UnauthorizedAccessException("Incorrect password.");
            }

            // 3. (Core) Generate JWT Token pass 🎫
            // Code here will be explained line by line later
            _logger.LogInformation($"✅ User '{request.Username}' logged in successfully");
            string token = CreateToken(user);
            return token;
        }

        public async Task<string> Register(UserDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return "Username already exists.";
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = Constants.Roles.User
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreateToken(user);
        }

        private string CreateToken(User user)
        {
            // A. Define "Payload" (Claims) - i.e., info written on the wristband
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username), // Wristband shows username
                new Claim(ClaimTypes.Role, user.Role),     // Wristband shows role (Admin/User)
                new Claim("id", user.Id.ToString())        // Wristband shows user ID
            };

            // B. Get the key (Read the Key configured in appsettings)
            // Note: In an actual project, this should use IConfiguration injection
            var secretKey = _configuration["MyJwtKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("JWT Secret Key is not configured.");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // C. Signing Credentials (Using HmacSha256 algorithm to sign)
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // D. Create Token
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Valid for 1 day
                signingCredentials: creds
            );

            // E. Convert the Token object to a string
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
