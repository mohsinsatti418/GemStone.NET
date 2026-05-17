using GemStonesApi.Interfaces;
using GemStonesApi.Repositories;
using GemStonesApi.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

builder.Services.AddScoped<IGemStoneRepository>(
    _ => new GemStoneRepository(connectionString));

builder.Services.AddScoped<IGemStoneService, GemStoneService>();

builder.Services.AddControllers();

builder.Services.Configure<
    Microsoft.AspNetCore.Http.Features.FormOptions > (o =>
    {
        o.MultipartBodyLengthLimit = 50 * 1024 * 1024;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();
app.Run();