using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PTN.WebAPI;
using PTN.WebAPI.EventBus;
using PTN.WebAPI.Extensions;
using PTN.WebAPI.Hubs;
using PTN.WebAPI.Mapping;
using PTN.WebAPI.Models;
using PTN.WebAPI.Repositories;
using PTN.WebAPI.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

// JWT Bearer Kimlik Doğrulama (Authentication) Yapılandırması
var jwtSettings = new JwtSettings();
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
});

// CORS Yapılandırması (appsettings.json içerisinden AllowedOrigins okunur)
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowConfiguredOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Swagger XML Dokümantasyon ve JWT Bearer Kilit Butonu Ayarı
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Swagger'a JWT Authorize kilit butonunu ekliyoruz
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Token değerinizi girin (Örnek: Bearer eyJhbGciOi...)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
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
// Statik dosyaları (wwwroot/swagger-custom.css) okumak için Build sonrası en üste alıyoruz:
app.UseStaticFiles();

// Swagger hem yerel ortamda hem de Docker ortamında her zaman aktif:
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.InjectStylesheet("../swagger-custom.css?v=999");
});

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
app.UseStaticFiles();
// Postman ve Tarayıcıdan gelen Accept-Language (en/tr) header'ını otomatik yakalama:
var supportedCultures = new[] { "tr-TR", "tr", "en-US", "en" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("tr-TR")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseRouting();
// CORS İznini Aktif Ediyoruz:
app.UseCors("AllowConfiguredOrigins");

// JWT Authentication ve Authorization Middleware Sırası
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<HealthHub>("/hubs/health");
app.MapControllers();

app.Run();