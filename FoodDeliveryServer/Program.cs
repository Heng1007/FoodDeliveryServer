using FoodDeliveryServer.Data;
using FoodDeliveryServer.Services;
using FoodDeliveryServer.Middleware; // 👈 记得引用这个命名空间
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 🏗️ 第一阶段：注册服务 (Builder 阶段)
// ==========================================

// 1. 注册 Controller
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 这一行是魔法！它让 Swagger 把 Enum 识别为字符串列表
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// 2. 注册数据库 (DbContext)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. 注册业务逻辑服务 (Dependency Injection)
builder.Services.AddScoped<IFoodService, FoodService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStatServices, StatServices>();

//【新增】注册后台托管服务
builder.Services.AddHostedService<FoodDeliveryServer.BackgroundServices.AutoOrderProcessor>();

// 4. 配置 CORS (允许跨域)
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

// 5. 配置 Swagger (带 JWT 锁头功能)
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
    throw new Exception("🚨 严重错误：在配置里找不到 'MyJwtKey'！请去 Azure 检查 Environment Variables！");
}

// 6. 配置 JWT 身份验证
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
// 🚧 分界线：房子盖好了 (Build)
// ==========================================
var app = builder.Build();

// ==========================================
// 🚦 第二阶段：配置管道 (App 阶段)
// ==========================================

// 1. 全局异常处理 (Day 24 重点：必须放在第一位！)
// 这样它才能捕获下面所有中间件发生的错误
app.UseMiddleware<ExceptionMiddleware>();

// 2. Swagger 界面 (开发环境可见)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. 静态文件 (Day 21 重点：允许访问 wwwroot)
app.UseDefaultFiles(); // 允许直接访问 index.html
app.UseStaticFiles();  // 允许访问 /images/xxx.jpg

// 4. HTTPS 重定向
app.UseHttpsRedirection();

// 5. CORS (必须在 Auth 之前)
app.UseCors(MyAllowSpecificOrigins);

// 6. 身份验证 & 授权 (顺序不能乱)
app.UseAuthentication(); // 你是谁？
app.UseAuthorization();  // 你能干嘛？

// 7. 映射控制器
app.MapControllers();

// 8. 启动程序
app.Run();