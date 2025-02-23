using API.Services;
using API.Services.Interfaces;
using API.Settings;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Register Redis
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));
builder.Services.AddScoped<IRedisService, RedisService>();

// Register HttpClient
builder.Services.AddHttpClient();

// Register Services in correct order
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<ICardProviderService, CardProviderService>();
builder.Services.AddScoped<ICreditCardService, CreditCardService>();

// Register Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Credit Card Recommendation API",
        Version = "v1",
        Description = "API for retrieving and scoring credit card recommendations from multiple providers"
    });

    // Hide schemas for error responses
    c.UseOneOfForPolymorphism();
    c.CustomSchemaIds(type => type.Name);

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Add this if you want to specify the port
builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
