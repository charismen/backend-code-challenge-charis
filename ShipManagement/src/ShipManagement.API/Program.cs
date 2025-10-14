using Microsoft.OpenApi.Models;
using ShipManagement.API.Data;
using ShipManagement.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ship Management API", Version = "v1" });
});

// Register services
builder.Services.AddSingleton<IDatabaseConnectionFactory>(sp => 
    new SqlConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IShipService, ShipService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICrewService, CrewService>();
builder.Services.AddScoped<IFinancialService, FinancialService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger");
        return;
    }

    await next();
});

app.MapControllers();

app.Run();
