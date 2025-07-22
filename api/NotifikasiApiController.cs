using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using mitraacd.Hubs;

namespace mitraacd.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotifikasiApiController : ControllerBase
    {
        private readonly IHubContext<NotifikasiHub> _hubContext;

        public NotifikasiApiController(IHubContext<NotifikasiHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public class NotifikasiRequest
        {
            public string Pesan { get; set; }
        }

        [HttpPost("kirim")]
        public async Task<IActionResult> KirimNotifikasi([FromBody] NotifikasiRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Pesan))
            {
                return BadRequest(new { error = "Pesan tidak boleh kosong." });
            }

            await _hubContext.Clients.All.SendAsync("TerimaNotifikasi", request.Pesan);

            return Ok(new { sukses = true, pesan = request.Pesan });
        }
    }
}


