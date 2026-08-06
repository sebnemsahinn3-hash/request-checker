using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PTN.WebAPI.Entities;

namespace PTN.WebAPI.EntityConfigurations
{
    public class RequestLogConfiguration : IEntityTypeConfiguration<RequestLogEntity>
    {
        public void Configure(EntityTypeBuilder<RequestLogEntity> builder)
        {
            // Tablo Adı (snake_case)
            builder.ToTable("request_logs");

            // Birincil Anahtar
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");

            // Sütun İsimleri (snake_case)
            builder.Property(x => x.Url)
                   .HasColumnName("url")
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.RequestParams)
                   .HasColumnName("request_params");

            builder.Property(x => x.RequestBody)
                   .HasColumnName("request_body");

            builder.Property(x => x.ResponseBody)
                   .HasColumnName("response_body");

            builder.Property(x => x.Timing)
                   .HasColumnName("timing")
                   .HasMaxLength(50);

            builder.Property(x => x.StatusCode)
                   .HasColumnName("status_code")
                   .IsRequired(false);

            builder.Property(x => x.Message)
                   .HasColumnName("message");

            builder.Property(x => x.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}