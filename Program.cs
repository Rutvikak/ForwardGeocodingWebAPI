using GeocodingAPI.Data;
using GeocodingAPI.Middlewares;
using GeocodingAPI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Initializing connectionstring and Nominatim API URL
string? connectionString = builder.Configuration.GetConnectionString("SQLiteConnectionstring");
string externalWebAPIURL = builder.Configuration["NominatimWebAPI:BaseWebAPI"];
string appinfo = $"{builder.Environment.ApplicationName} / {builder.Configuration["ApplicationInfo:Version"]} ({builder.Configuration["ApplicationInfo:emailId"]})";

// Setting Serilog as logger
builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
// Registering "GeocodingAPIDbContext" (derived from DbContext class) with Services for providing it's objects using DI 
if (connectionString != null)
{
    builder.Services.AddDbContext<GeocodingAPIDbContext>(opt => opt.UseSqlite(connectionString));
}

// Registering "GeocodingServices" class with Services with AddScoped() for injecting object of the same per request
builder.Services.AddScoped<IGeocodingServices, GeocodingServices>();

if (externalWebAPIURL != null)
{
    builder.Services.AddHttpClient<IGeocodingServices, GeocodingServices>(client =>
    {
        client.BaseAddress = new Uri(externalWebAPIURL);
        client.DefaultRequestHeaders.Add("User-Agent", $"{appinfo}");
    });
}

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.MapControllers();

Log.Information("Geocoding API Started!");
app.Run();
