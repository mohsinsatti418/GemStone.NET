using GemStonesApi.Interfaces;
using GemStonesApi.Repositories;
using GemStonesApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://*:{port}");

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

// ?? GemStone DI ??????????????????????????????????????????
builder.Services.AddScoped<IGemStoneRepository>(
    _ => new GemStoneRepository(connectionString));
builder.Services.AddScoped<IGemStoneService, GemStoneService>();

// ?? Auth DI ???????????????????????????????????????????????
builder.Services.AddScoped<IAuthRepository>(
    _ => new AuthRepository(connectionString));
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddSingleton<IAzureBlobService, AzureBlobService>();

builder.Services.AddControllers();

// ?? File upload limit ?????????????????????????????????????
builder.Services.Configure<
    Microsoft.AspNetCore.Http.Features.FormOptions > (o =>
    {
        o.MultipartBodyLengthLimit = 50 * 1024 * 1024;
    });

// ?? CORS — your existing policy kept exactly ?????????????
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            
                policy.WithOrigins("https://gem-stone-react.vercel.app")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// ?? JWT Authentication ????????????????????????????????????
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();

// ?? Swagger ???????????????????????????????????????????????
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // This adds the Authorize button in Swagger UI
    // so you can test protected endpoints with a token
    c.AddSecurityDefinition("Bearer", new
        Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description =
            "Paste your token here. Example: eyJhbGci..."
    });

    c.AddSecurityRequirement(new
        Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseStaticFiles();

// ?? Middleware order — this order is mandatory ????????????
app.UseCors("AllowFrontend");         // your existing CORS
app.UseAuthentication();              // NEW — who are you?
app.UseAuthorization();               // NEW — what can you do?

app.MapControllers();
app.Run();