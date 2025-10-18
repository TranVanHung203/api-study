using Entities.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Service;
using Service.Contracts;
using Contracts;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using qlnv;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Connection string MySQL (appsettings.json)
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<RepositoryContext>(options =>
    options.UseMySql(conn, ServerVersion.AutoDetect(conn)));

// 🔹 DI Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INgayLeRepository, NgayLeRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<Contracts.INhanVienRepository, Repository.NhanVienRepository>();
builder.Services.AddScoped<Contracts.IEmailThongBaoRepository, Repository.EmailThongBaoRepository>();
builder.Services.AddScoped<Contracts.ICauHinhThongBaoRepository, Repository.CauHinhThongBaoRepository>();
builder.Services.AddScoped<Contracts.IThongBaoRepository, Repository.ThongBaoRepository>();

// 🔹 DI Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<INgayLeService, NgayLeService>();
builder.Services.AddScoped<INhanVienService, NhanVienService>();
builder.Services.AddScoped<Service.Contracts.IEmailThongBaoService, Service.EmailThongBaoService>();
builder.Services.AddScoped<Service.Contracts.ICauHinhThongBaoService, Service.CauHinhThongBaoService>();
builder.Services.AddScoped<Service.Contracts.IThongBaoService, Service.ThongBaoService>();
// Register scheduled hosted service to run notifications daily at 08:00 local time
builder.Services.AddHostedService<Service.CauHinhThongBaoScheduledService>();
// Register birthday notification service to run on 1st of each month at 08:00 local time
builder.Services.AddHostedService<Service.BirthdayScheduledService>();
// //Provide access to HttpContext for background services when necessary
builder.Services.AddHttpContextAccessor();
// Hosted background job removed: notifications will be triggered manually via controller
// 🔹 JWT
var jwt = builder.Configuration.GetSection("JwtSettings");
var keyString = jwt["Key"] ?? throw new InvalidOperationException("JwtSettings:Key is not configured.");
var key = Encoding.UTF8.GetBytes(keyString);

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
        ValidateLifetime = true,
        // Make lifetime check strict (no default 5 minute clock skew)
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    // Additionally, explicitly inspect the token's "exp" claim on receipt and reject immediately if expired.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Token;
            if (string.IsNullOrEmpty(token))
            {
                return Task.CompletedTask;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
                if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out var expSeconds))
                {
                    var exp = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
                    if (exp <= DateTimeOffset.UtcNow)
                    {
                        // Mark the token as failed/expired so authentication short-circuits
                        context.Fail("Token expired");
                    }
                }
            }
            catch
            {
                // If parsing fails, allow normal validation pipeline to handle invalid tokens.
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers(options =>
{
    // Require authenticated users by default for all controllers
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
})
    .AddApplicationPart(typeof(qlnv.Presentation.AssemblyReference).Assembly)
    .AddJsonOptions(opts =>
    {
        // Force DateTime serialization to UTC ISO 8601 with 'Z'
        opts.JsonSerializerOptions.Converters.Add(new qlnv.JsonConverters.DateTimeUtcConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\nExample: \"Bearer eyJhbGciOi...\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



var app = builder.Build();


// Global exception handler (must be early)
app.UseGlobalExceptionHandler();

app.UseCors("AllowAngularClient");

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthentication(); // phải trước UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.Run();
