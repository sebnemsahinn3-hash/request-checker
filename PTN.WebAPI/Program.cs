using Microsoft.EntityFrameworkCore;
using PTN.WebAPI;
using PTN.WebAPI.Extensions;
using PTN.WebAPI.Mapping;
using PTN.WebAPI.Repositories;
using PTN.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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
 // Swagger XML Dokümantasyon Ayarı
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// PostgreSQL DbContext Kaydı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Redis Önbellek (Distributed Cache) Servis Kaydı
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "PTN_HealthCheck_";
});

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.InjectStylesheet("../swagger-custom.css?v=999");// Bir üst klasördeki CSS yolunu verir!
    });
}

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
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();