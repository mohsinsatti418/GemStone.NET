using GemStonesApi.Interfaces;
using GemStonesApi.Repositories;
using GemStonesApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://*:{port}");

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

// ?? GemStone DI ??????????????????????????????????????????
builder.Services.AddScoped<IGemStoneRepository>(
    _ => new GemStoneRepository(connectionString));
builder.Services.AddScoped<IGemStoneService, GemStoneService>();

// ?? Captcha DI ????????????????????????????????????????????
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICaptchaService, CaptchaService>();

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

builder.Services.AddRateLimiter(options =>
{
    // Global policy — applies to all endpoints
    options.GlobalLimiter = PartitionedRateLimiter
        .Create<HttpContext, string>(context =>
        {
            // Rate limit per IP address
            var ipAddress = context.Connection.RemoteIpAddress?
                .ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ipAddress,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,  // 100 requests
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
        });

    // Strict policy — for login and register only
    options.AddPolicy("StrictLimit", context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?
            .ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,   // only 5 attempts
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0
            });
    });

    // When limit is exceeded return 429 Too Many Requests
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.",
            token);
    };
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
// ?? Security Headers ??????????????????????????????????????
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;

    // Prevents clickjacking
    headers["X-Frame-Options"] = "DENY";

    // Prevents MIME type sniffing
    headers["X-Content-Type-Options"] = "nosniff";

    // Forces HTTPS for 1 year
    headers["Strict-Transport-Security"] =
        "max-age=31536000; includeSubDomains";

    // Prevents referrer leaking
    headers["Referrer-Policy"] =
        "strict-origin-when-cross-origin";

    // Restricts browser features
    headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=()";

    // Basic Content Security Policy
    headers["X-XSS-Protection"] = "1; mode=block";

    await next();
});
app.UseStaticFiles();

// ?? Middleware order — this order is mandatory ????????????
app.UseCors("AllowFrontend");         // your existing CORS
app.UseRateLimiter();
app.UseAuthentication();              // NEW — who are you?
app.UseAuthorization();               // NEW — what can you do?

app.MapControllers();
app.Run();