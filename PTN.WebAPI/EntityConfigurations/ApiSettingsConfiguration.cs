using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PTN.WebAPI.Entities;

namespace PTN.WebAPI.EntityConfigurations
{
    public class ApiSettingsConfiguration : IEntityTypeConfiguration<ApiSettings>
    {
        public void Configure(EntityTypeBuilder<ApiSettings> builder)
        {
            // Tablo Adı (snake_case)
            builder.ToTable("api_settings");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");

            builder.Property(x => x.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Url)
                   .HasColumnName("url")
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.ApiKey)
                   .HasColumnName("api_key");

            builder.HasData(new ApiSettings
            {
                Id = 1,
                Name = "GTFS Check API",
                Url = "https://ventral-vivan-brinkless.ngrok-free.dev/api/gtfs/check",
                ApiKey = "sample-api-key"
            });
        }
    }
}