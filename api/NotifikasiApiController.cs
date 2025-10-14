using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using mitraacd.Hubs;
using mitraacd.Models;

namespace mitraacd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotifikasiController : ControllerBase
    {
        private readonly IHubContext<NotifikasiHub> _hubContext;

        public NotifikasiController(IHubContext<NotifikasiHub> hubContext)
        {
            _hubContext = hubContext;
        }

        

    }
}
