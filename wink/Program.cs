using wink.Models;
using wink.Services;

var builder = WebApplication.CreateBuilder(args);

//configure mongoDB settings and add to DI Container
builder.Services.Configure<WinkDatabaseSettings>(builder.Configuration.GetSection("WinkDatabase"));

// Add services to the DI container.
builder.Services.AddSingleton<UserService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
