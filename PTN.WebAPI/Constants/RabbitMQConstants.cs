namespace PTN.WebAPI.Constants
{
    public static class RabbitMQConstants
    {
        public const string QueueName = "critical_health_alert_queue";
        public const string ExchangeName = "health_events_exchange";
        public const string RoutingKey = "health.critical";
    }
}