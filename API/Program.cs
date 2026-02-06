using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Npgsql;
using Repository.Interfaces;
using Repository.Service;
using Repository.Services;
using Repository.service;
using Repository.Implementation;
using StackExchange.Redis;
using Stripe;
using API.Services;
using System.Text;
using Repository.Model;
using Nest;
using Microsoft.AspNetCore.DataProtection;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ===================== CORE SERVICES =====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddControllersWithViews();
// RabbitMQ options and service (kept for now)
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RabbitMqService>();

// Redis options and services
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
var redisConfig = builder.Configuration.GetSection("Redis").GetValue<string>("Configuration");
IConnectionMultiplexer? redisConnection = null;

if (!string.IsNullOrEmpty(redisConfig))
{
    try
    {
        redisConnection = ConnectionMultiplexer.Connect(redisConfig);
        builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);
    }
    catch
    {
        // Redis connection failed, will use fallback
        redisConnection = null;
    }
}

if (redisConnection == null)
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RedisOptions>>().Value;
        return ConnectionMultiplexer.Connect(opts.Configuration);
    });
}

builder.Services.AddSingleton<IChatService, RedisChatService>();

// ===================== DATA PROTECTION (MUST BE BEFORE SESSION) =====================
// Configure Data Protection to persist keys (fixes session cookie decryption errors)
// Using file system persistence (works reliably without additional packages)
// var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");
// Directory.CreateDirectory(dataProtectionKeysPath);

// builder.Services.AddDataProtection()
//     .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
//     .SetApplicationName("DineSwift");

// Session via Redis (configured AFTER Data Protection)
builder.Services.AddStackExchangeRedisCache(options =>
{
    var cfg = builder.Configuration.GetSection("Redis").GetValue<string>("Configuration");
    var instance = builder.Configuration.GetSection("Redis").GetValue<string>("InstanceName") ?? "MVC:";
    options.Configuration = cfg;
    options.InstanceName = instance + "session:";
});
// ===================== SWAGGER =====================
// Add services to the container.
builder.Services.AddControllersWithViews();
// RabbitMQ options and service (kept for now)
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RabbitMqService>();

// Redis options and services
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RedisOptions>>().Value;
    return ConnectionMultiplexer.Connect(opts.Configuration);
});
builder.Services.AddSingleton<IChatService, RedisChatService>();
builder.Services.AddDistributedMemoryCache();

// Session via Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    var cfg = builder.Configuration.GetSection("Redis").GetValue<string>("Configuration");
    var instance = builder.Configuration.GetSection("Redis").GetValue<string>("InstanceName") ?? "MVC:";
    options.Configuration = cfg;
    options.InstanceName = instance + "session:";
});
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("token", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = HeaderNames.Authorization
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "token"

                }
            },
            Array.Empty<string>()
        }
    });
});

// ===================== STRIPE =====================
StripeConfiguration.ApiKey =
    builder.Configuration["Stripe:SecretKey"]
    ?? throw new Exception("Stripe SecretKey missing");

// ===================== JWT KEY =====================
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new Exception("JWT Key missing");

// ===================== CACHE =====================
builder.Services.AddMemoryCache();
// Note: Distributed cache is configured above via Redis (AddStackExchangeRedisCache)
// Do NOT add AddDistributedMemoryCache() as it will override Redis cache

// Persist data-protection keys so session cookies can be unprotected across restarts
// var dataProtectionKeysPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DineSwift-DataProtection-Keys");
// System.IO.Directory.CreateDirectory(dataProtectionKeysPath);
// builder.Services.AddDataProtection()
//     .PersistKeysToFileSystem(new System.IO.DirectoryInfo(dataProtectionKeysPath))
//     .SetApplicationName("DineSwiftApp");

// ===================== DEPENDENCY INJECTION =====================

builder.Services.AddScoped<IRestaurantRegisterService, RestaurantRegisterService>();

builder.Services.AddScoped<IEmailService, EmailService>();


builder.Services.AddScoped<IRatingServices, RatingServices>();



builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminInterface, AdminService>();

// ✅ FIX: Fully qualify CustomerService to avoid Stripe conflict
builder.Services.AddScoped<ICustomerService, Repository.service.CustomerService>();
builder.Services.AddScoped<IUserHomeServices, UserHomeServices>();
builder.Services.AddScoped<IDeliveryService, deliveryservice>();
builder.Services.AddScoped<IFoodItemServices, FoodItemServices>();
builder.Services.AddScoped<Repository.service.IEmailService, Repository.Services.EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<API.service.IOrderInterface, API.service.OrderService>();
builder.Services.AddScoped<API.service.IOrderHistoryInterface, API.service.OrderHistoryService>();
builder.Services.AddScoped<IRestaurantProfileMenu, RestaurantProfileMenu>();
builder.Services.AddScoped<IPromocodeService, PromocodeService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IForgotService, ForgotService>();
builder.Services.AddScoped<IPasswordRepository, PasswordRepository>();
builder.Services.AddScoped<IElasticSearch, ElasticSearch>();
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();

builder.Services.AddScoped<IDeliveryProfileService, DeliveryProfileService>();
builder.Services.AddScoped<NpgsqlConnection>(st =>
{
    var connectionString =
        st.GetRequiredService<IConfiguration>().GetConnectionString("pgconn");
    return new NpgsqlConnection(connectionString);
});

// Session configuration (Data Protection must be configured BEFORE this)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    // Allow session to work even if cookie decryption fails (graceful degradation)
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Set to SameAsRequest in production
});


// =============== ELASCTIC SEARCH ==============

builder.Services.AddScoped<IElasticClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var url = configuration["ElasticsearchSettings:Url"];
    var defaultIndex = configuration["ElasticsearchSettings:DefaultIndex"];
    var username = configuration["ElasticsearchSettings:Username"];
    var password = configuration["ElasticsearchSettings:Password"];


    var settings = new ConnectionSettings(new Uri(url))
        .DefaultIndex(defaultIndex);

    if (!string.IsNullOrEmpty(username))
    {
        settings = settings.BasicAuthentication(username, password);
    }

    return new ElasticClient(settings);
});

// ===================== JWT AUTH =====================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ===================== CORS =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());

    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:4200",
                "http://localhost:5030",
                "http://localhost:5245",
                "http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

// ===================== MIDDLEWARE =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Skip HTTPS redirection in development when running on HTTP to avoid warnings
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseCors("AllowAll");

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ===================== WEATHER FORECAST =====================
string[] summaries =
{
    "Freezing","Bracing","Chilly","Cool","Mild",
    "Warm","Balmy","Hot","Sweltering","Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    return Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )
    ).ToArray();
})
.WithName("GetWeatherForecast")
.WithOpenApi();



async Task IndexDataOnStartup()
{
    using var scope = app.Services.CreateScope();
    var itemRepo = scope.ServiceProvider.GetRequiredService<IFoodItemServices>();
    var esService = scope.ServiceProvider.GetRequiredService<IElasticSearch>();
    var rsService = scope.ServiceProvider.GetRequiredService<IRestaurantRepository>();

    try
    {
        await esService.CreateFoodItemIndexAsync();
        await esService.CreateRestaurantIndexAsync();
        List<t_fooditem> items = await itemRepo.GetAllFoodItem();
        List<t_vm_restaurant> restaurants = await rsService.GetAllRestaurants();  
        if (items.Count > 0)
        {
            foreach (t_fooditem item in items)
            {
                await esService.addFoodItems(item);
            }
            Console.WriteLine($" {items.Count} Food Items indexed successfully in ElasticSearch.");
        }
        else
        {
            Console.WriteLine(" No Food Items found in PostgreSQL.");
        }

        if (restaurants.Count > 0)
        {
            foreach (t_vm_restaurant item in restaurants)
            {
                await esService.addRestaurant(item);
            }
            Console.WriteLine($" {restaurants.Count} Restaurant indexed successfully in ElasticSearch.");
        }
        else
        {
            Console.WriteLine(" No Restaurant found in PostgreSQL.");
        }

    }
    catch (Exception e)
    {
        Console.WriteLine($" Error indexing contacts: {e.Message}");
    }
}

await IndexDataOnStartup();


app.Run();

// ===================== MODEL =====================
record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string Summary);
