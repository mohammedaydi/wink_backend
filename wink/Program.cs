using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using wink.Models;
using wink.Services;

var builder = WebApplication.CreateBuilder(args);

//configure mongoDB settings and add to DI Container
builder.Services.Configure<WinkDatabaseSettings>(builder.Configuration.GetSection("WinkDatabase"));

// Add services to the DI container.
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ItemService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<CartService>();
builder.Services.AddSingleton<ItemCartService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// JWT Authentication configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // e.g., "yourdomain.com"
            ValidAudience = builder.Configuration["Jwt:Audience"], // e.g., "yourdomain.com"
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

//adding cors
builder.Services.AddCors(options =>
{
    //options.AddPolicy("AllowSpecificOrigins", policy =>
    //{
    //    policy.WithOrigins("https://example.com") 
    //          .AllowAnyHeader()
    //          .AllowAnyMethod();
    //});

    // Optional: Add a policy to allow all origins (not recommended for production)
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



//build the app
var app = builder.Build();


app.UseCors("AllowAllOrigins");

// Use Authentication and Authorization middleware --added with jwt
app.UseAuthentication();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
