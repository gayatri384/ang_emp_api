using ang_emp_api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using ang_emp_api.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using ang_emp_api.Services;


var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.AddScoped<IEmailService, EmailService>();

// Add services to the container.
// add attendance service
builder.Services.AddScoped<AttendanceService>();

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        // This is the key line
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        x.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();   // required for swagger
builder.Services.AddSwaggerGen();             // required for swagger


// for JWT Token
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})


.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    // Single-session enforcement: compare token sessionId with DB sessionId
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

            var userIdClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var sessionClaim = context.Principal?.FindFirst("sessionId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(sessionClaim))
            {
                context.Fail("Invalid token");
                return;
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Fail("Invalid user id");
                return;
            }

            var user = await db.Employees.FindAsync(userId);
            if (user == null || user.SessionId != sessionClaim)
            {
                context.Fail("Session invalid or expired");
                return;
            }
        }
    };
});

// Add CORS service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Angular app URL
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Swagger: add JWT UI support
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// send assets mail
builder.Services.AddScoped<EmailService>();

var app = builder.Build();
// Use CORS before MapControllers
app.UseCors("AllowAngular");

// Serve static files
app.UseStaticFiles();
app.UseRouting();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
    
app.MapControllers();

// Fall back to Angular index.html for client-side routes
//D:\INTERNSHIP\Angular\ang_emp_api\ang_emp_api\wwwroot\login\index.html
app.MapFallbackToFile("login/index.html");


app.Run();
