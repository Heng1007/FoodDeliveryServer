using FoodDeliveryServer.Data;
using FoodDeliveryServer.Services;
using FoodDeliveryServer.Middleware; // 👈 Remember to reference this namespace
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 🏗️ Phase 1: Register Services (Builder Phase)
// ==========================================

// 1. Register Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // This line is magic! It allows Swagger to recognize Enum as a list of strings
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// 2. Register Database (DbContext)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Register Business Logic Services (Dependency Injection)
builder.Services.AddScoped<IFoodService, FoodService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStatServices, StatServices>();

// [New] Register Hosted Background Services
builder.Services.AddHostedService<FoodDeliveryServer.BackgroundServices.AutoOrderProcessor>();

// 4. Configure CORS (Cross-Origin Resource Sharing)
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:5173",
                "https://food-delivery-client-62vj.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

// 5. Configure Swagger (With JWT lock feature)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var jwtKey = builder.Configuration["MyJwtKey"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("🚨 Critical Error: 'MyJwtKey' cannot be found in configuration! Please check Environment Variables in Azure!");
}

// 6. Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ==========================================
// 🚧 Boundary: The house is built (Build)
// ==========================================
var app = builder.Build();

// ==========================================
// 🚦 Phase 2: Configure Pipeline (App Phase)
// ==========================================

// 1. Global Exception Handling (Must be placed first!)
// This allows it to catch errors from all subsequent middlewares
app.UseMiddleware<ExceptionMiddleware>();

// 2. Swagger UI (Visible in Development environment)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. Static Files (Allow access to wwwroot)
app.UseDefaultFiles(); // Allow direct access to index.html
app.UseStaticFiles();  // Allow access to /images/xxx.jpg

// 4. HTTPS Redirection
app.UseHttpsRedirection();

// 5. CORS (Must be before Auth)
app.UseCors(MyAllowSpecificOrigins);

// 6. Authentication & Authorization (Order is strict)
app.UseAuthentication(); // Who are you?
app.UseAuthorization();  // What can you do?

// 7. Map Controllers
app.MapControllers();

// 8. Run the Application
app.Run();