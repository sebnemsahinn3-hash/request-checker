using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PTN.WebAPI.Constants;
using PTN.WebAPI.Events;
using PTN.WebAPI.Repositories;
using PTN.WebAPI.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PTN.WebAPI.EventBus
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public RabbitMQConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                    Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672")
                };

                var connection = await factory.CreateConnectionAsync(stoppingToken);
                var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: RabbitMQConstants.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken
                );

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    var alertEvent = JsonSerializer.Deserialize<CriticalHealthAlertEvent>(messageJson);

                    if (alertEvent != null)
                    {
                        Console.WriteLine($"[RABBITMQ CONSUMED] Kuyruktan Mesaj Alındı: {alertEvent.Title}");

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                            // Veritabanındaki tüm aktif kullanıcıları çekiyoruz
                            var users = await userRepository.GetAllUsersAsync();
                            var activeUserEmails = users.Where(u => u.IsActive).Select(u => u.Email).ToList();

                            if (activeUserEmails.Any())
                            {
                                string htmlBody = $"<h3>{alertEvent.Title}</h3><p>{alertEvent.Message}</p><br/><small>Zaman: {alertEvent.AlertTime}</small>";
                                await emailService.SendBulkEmailAsync(activeUserEmails, alertEvent.Title, htmlBody);
                            }
                        }
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
                };

                await channel.BasicConsumeAsync(queue: RabbitMQConstants.QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ Consumer Hatası: {ex.Message}");
            }
        }
    }
}