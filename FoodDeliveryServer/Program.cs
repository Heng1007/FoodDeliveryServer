using FoodDeliveryServer.Data;
using FoodDeliveryServer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 注册数据库服务
// 翻译："老板，请雇佣 AppDbContext，并让它使用 SQL Server，
// 地址去配置文件里找那个叫 'DefaultConnection' 的字符串。"
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IFoodService, FoodService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Add services to the container.
builder.Services.AddControllers();

// 【修改点 1】: 换回经典的 Swagger 生成器
// 之前的 AddOpenApi() 是新版的，我们不用它
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // 【修改点 2】: 显式启用 Swagger 界面
    // 这两行就是那个 "蓝色网页" 的开关
    app.UseSwagger();
    app.UseSwaggerUI(); 
}

//允许所有人访问 wwwroot 文件夹里的静态文件 (html, css, js)
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// 我正在测试 Visual Studio 的 Git 功能！