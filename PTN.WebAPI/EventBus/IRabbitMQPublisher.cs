using PTN.WebAPI.Events;
using System.Threading.Tasks;

namespace PTN.WebAPI.EventBus
{
    public interface IRabbitMQPublisher
    {
        Task PublishCriticalAlertAsync(CriticalHealthAlertEvent alertEvent);
    }
}