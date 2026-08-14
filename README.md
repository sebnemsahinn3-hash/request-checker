# Request Checker - Backend

Bu depo, yazılım stajı kapsamında geliştirilen API performans izleme projesinin ASP.NET Core Web API kaynak kodlarını içerir.

## Kullanılan teknolojiler

- C# ve ASP.NET Core Web API
- PostgreSQL ve Entity Framework Core
- Redis önbellekleme
- SignalR ile gerçek zamanlı bildirim
- JWT kimlik doğrulama ve rol bazlı yetkilendirme
- RabbitMQ ile asenkron mesajlaşma
- SMTP e-posta bildirimi
- Docker ve Docker Compose

## Güvenli yapılandırma

Teslim branch'inde gerçek parola, JWT anahtarı ve SMTP hesabı bulunmaz. Uygulamayı çalıştırmadan önce `PTN.WebAPI/appsettings.json` içindeki örnek alanlar yerel ortama göre düzenlenmelidir.

SMTP bilgileri kaynak koda yazılmamalı; `Smtp__User` ve `Smtp__Password` ortam değişkenleri kullanılmalıdır.

## Çalıştırma

```powershell
cd PTN.WebAPI
dotnet restore
dotnet run
```

Docker kullanmak için:

```powershell
cd PTN.WebAPI
dotnet publish -c Release -o publish
docker compose up --build
```

Arayüz deposu: [request-checker-ui](https://github.com/sebnemsahinn3-hash/request-checker-ui/tree/staj-teslim)
