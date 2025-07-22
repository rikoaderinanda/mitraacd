using Microsoft.AspNetCore.SignalR;

namespace mitraacd.Hubs
{
    public class NotifikasiHub : Hub
    {
        public async Task KirimNotifikasi(string pesan)
        {
            await Clients.All.SendAsync("TerimaNotifikasi", pesan);
        }
    }
}