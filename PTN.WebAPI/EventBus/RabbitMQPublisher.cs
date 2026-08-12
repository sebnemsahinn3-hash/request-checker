using Microsoft.Extensions.Configuration;
using PTN.WebAPI.Constants;
using PTN.WebAPI.Events;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PTN.WebAPI.EventBus
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly IConfiguration _configuration;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task PublishCriticalAlertAsync(CriticalHealthAlertEvent alertEvent)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                    Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672")
                };

                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: RabbitMQConstants.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var messageJson = JsonSerializer.Serialize(alertEvent);
                var body = Encoding.UTF8.GetBytes(messageJson);

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: RabbitMQConstants.QueueName,
                    body: body
                );

                Console.WriteLine($"[RABBITMQ PUSHED] Olay Kuyruğa Atıldı: {alertEvent.Title}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ Mesaj Yollama Hatası: {ex.Message}");
            }
        }
    }
}