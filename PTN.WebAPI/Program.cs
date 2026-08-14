using FluentValidation;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PTN.WebAPI;
using PTN.WebAPI.Constants;
using PTN.WebAPI.EventBus;
using PTN.WebAPI.Extensions;
using PTN.WebAPI.Hubs;
using PTN.WebAPI.Mapping;
using PTN.WebAPI.Models;
using PTN.WebAPI.Repositories;
using PTN.WebAPI.Services;
using System.Text;
using Microsoft.AspNetCore.Identity;
using PTN.WebAPI.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<
    IValidator<RequestLogDeleteDto>,
    RequestLogDeleteDtoValidator>();
builder.Services.AddScoped<
    IPasswordHasher<UserEntity>,
    PasswordHasher<UserEntity>>();
// SignalR Canlı Bildirim Servisi Kaydı
builder.Services.AddSignalR();

// Kullanıcı Yönetimi ve Auth Bağımlılık Kayıtları
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IAuthService, AuthService>();

// RabbitMQ ve SMTP E-Posta Servis Kayıtları
builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddHostedService<RabbitMQConsumer>();
builder.Services.AddScoped<
    IHealthService,
    HealthService>();

// JWT Bearer Kimlik Doğrulama (Authentication) Yapılandırması
var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddSingleton(jwtSettings);
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json; charset=utf-8";
            var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Program>>();
            var message = localizer[AuthConstants.Unauthorized];
            await context.Response.WriteAsJsonAsync(new { status = 401, message = message.Value });
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json; charset=utf-8";
            var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Program>>();
            var message = localizer[AuthConstants.Forbidden];
            await context.Response.WriteAsJsonAsync(new { status = 403, message = message.Value });
        }
    };
});

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowConfiguredOrigins",
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});
// Swagger XML Dokümantasyon ve JWT Bearer Kilit Butonu Ayarı
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Swagger'a JWT Authorize kilit butonunu ekliyoruz
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Token değerinizi girin (Örnek: Bearer eyJhbGciOi...)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// PostgreSQL DbContext Kaydı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Önbellek (Distributed Cache) Servis Kaydı: Yerel MemoryCache Fallback Aktif!
builder.Services.AddDistributedMemoryCache();

// BackgroundService, Service ve Repository Kayıtları
builder.Services.AddHostedService<HealthCheckBackgroundService>();
builder.Services.AddTransient<IRequestService, RequestService>();
builder.Services.AddScoped<IRequestRepository, EfRequestRepository>();
builder.Services.AddCustomLocalization(); 
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "tr-TR", "en-US" };
    options.SetDefaultCulture("tr-TR") // Varsayılan dil Türkçe
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

app.UseRequestLocalization();
app.UseStaticFiles();

// Swagger Middleware Yapılandırması
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.InjectStylesheet("/swagger-custom.css");
});

// Ana adrese (/) gelindiğinde otomatik olarak /swagger adresine yönlendir:
app.MapGet("/", () => Results.Redirect("/swagger"));

// VERİTABANI OTOMATİK MIGRATION UYGULAMA
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Postman ve Tarayıcıdan gelen Accept-Language (en/tr) header'ını otomatik yakalama:
var supportedCultures = new[] { "tr-TR", "tr", "en-US", "en" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("tr-TR")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseRouting();
// CORS İznini Aktif Ediyoruz:
app.UseCors("AllowConfiguredOrigins");
app.UseMiddleware<PTN.WebAPI.Extensions.ApiResponseMiddleware>();

// JWT Authentication ve Authorization Middleware Sırası
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<HealthHub>("/hubs/health");
app.MapControllers();

app.Run();