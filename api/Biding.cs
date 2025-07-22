using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mitraacd.Models;
using mitraacd.Services;

namespace mitraacd.api
{
    [ApiController]
    [Route("api/[controller]")]
    public class BidingController : ControllerBase
    {
        private readonly IBidRepository _bidRepository;

        public BidingController(IBidRepository bidRepository)
        {
            _bidRepository = bidRepository;
        }

        [HttpGet("GetBid")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetBid()
        {
            var res = await _bidRepository.GetBidAsync();
            return Ok(res);
        }

        [HttpPost("Takeit")]
        public async Task<IActionResult> Takeit([FromBody] TakeBidingModel dto)
        {
            if (dto == null || dto.Id == null)
                return BadRequest(new { message = "Pemesanan sudah tidak tersedia" });
            try
            {
                var res = await _bidRepository.TakeitAsync(dto);
                return Ok(new { message = "Biding berhasil disimpan", id = res });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan Biding"});
            }
        }
    }
}