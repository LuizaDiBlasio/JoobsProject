using System.Text.Json.Serialization;
using JobPortal_API.Data;
using JobPortal_API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using JobPortal_API.Repository;
using JobPortal_API.Repository.IRepository;
using JobPortal_API.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using JobPortal_API.Filters;
using System.Security.Claims;
using System.Text;
using JobPortal_API.Utilities.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:5070") // Your Razor Pages URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 2. Add DbContext & Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 3. Merged Authentication & JWT Bearer Configuration
var jwtSecretKey = builder.Configuration["Jwt:Key"] ?? "minha-chave-jwt-supersecreta";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),

        // Validation rules (Adjust true/false based on your strictness needs)
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = "JobPortalAPI",
        ValidAudience = "JobPortalAPI",

        // Map roles correctly for [Authorize(Roles="Admin")]
        RoleClaimType = ClaimTypes.Role
    };
});

// 4. Register Services & Repositories
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<VerificaCandidatoFilter>();
builder.Services.AddScoped<VerificaEmpresaFilter>();
builder.Services.AddScoped<VerificaOfertaDeEmpresaFilter>();
builder.Services.AddScoped<IMailHelper, MailHelper>();
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddTransient<SeedDB>();

builder.Services.AddResponseCaching();
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

// 5. Add Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve; // Prevents circular reference issues
    });

// 6. Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "JobPortal_API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Digite 'Bearer' seguido do token JWT.\n\nExemplo: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Swagger is being configured...");
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "JobPortal_API V1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    Console.WriteLine("Swagger is not configured in production.");
}

app.UseStaticFiles();
app.UseRouting();
// Habilitar Seed 
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<SeedDB>();
    await seeder.SeedAsync();
}


app.UseHttpsRedirection();

// IMPORTANT: CORS must be between UseRouting and UseAuthentication
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();