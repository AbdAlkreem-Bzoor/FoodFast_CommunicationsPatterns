using FoodFast.API.Background;
using FoodFast.API.Data;
using FoodFast.API.Domain.Entities;
using FoodFast.API.Domain.Models;
using FoodFast.API.Hubs;
using FoodFast.API.Infrastructure;
using FoodFast.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FoodFastDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("SqlServer")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
}).AddEntityFrameworkStores<FoodFastDbContext>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var host = config["Redis:Host"] ?? "redis";
    var port = config["Redis:Port"] ?? "6379";
    return ConnectionMultiplexer.Connect($"{host}:{port}");
});

builder.Services.AddSingleton<ITokenBlacklistService, RedisTokenBlacklistService>();

builder.Services.Configure<RabbitMqSetting>(
    builder.Configuration.GetSection("RabbitMq:RabbitMqAnnouncementSettings"));

builder.Services.AddOptions<EmailSettings>()
    .BindConfiguration("EmailSettings");


builder.Services.AddSingleton<IRabbitMqAnnouncementPublisher, RabbitMqAnnouncementPublisher>();

builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();

builder.Services.AddHostedService<RabbitMqAnnouncementConsumer>();

builder.Services.AddSignalR();

builder.Services.Configure<TokenSettings>
    (builder.Configuration.GetSection("JWT"));

var jwtKey = config["JWT:Key"]!;
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["JWT:Issuer"],
        ValidAudience = config["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateLifetime = true
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var accessToken = ctx.Request.Query["access_token"].FirstOrDefault();
            var path = ctx.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs") ||
                path.StartsWithSegments("/api/orders") ||
                path.StartsWithSegments("/api/announcements")))
            {
                ctx.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async ctx =>
        {
            var jti = ctx.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrEmpty(jti))
            {
                var blacklist = ctx.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
                if (await blacklist.IsBlacklistedAsync(jti))
                {
                    ctx.Fail("Token revoked");
                }
            }
        }
    };
});

builder.Services.AddCors(options => options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<FoodFastDbContext>();
    database.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DeliveryHub>("/hubs/delivery");
app.MapHub<RestaurantHub>("/hubs/restaurant");
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<AnnouncementsHub>("/hubs/announcements");
app.MapHub<OrderHub>("/hubs/orders");



app.Run();
