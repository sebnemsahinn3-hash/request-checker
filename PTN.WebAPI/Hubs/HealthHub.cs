using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PTN.WebAPI.Hubs
{
    public class HealthHub : Hub
    {
        // Canlı istemcilerin (frontend) bağlandığını doğrular
        public async Task SendHealthAlert(string title, string message)
        {
            await Clients.All.SendAsync("ReceiveCriticalHealthAlert", title, message);
        }
    }
}