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
        public async Task<ActionResult<IEnumerable<dynamic>>> GetBid(string Id)
        {
            var res = await _bidRepository.GetBidAsync(Id);
            return Ok(res);
        }

        [HttpPost("Takeit")]
        public async Task<IActionResult> Takeit([FromBody] TakeBidingModel req)
        {
            var res = await _bidRepository.Takeit(req);
            if (res)
            {
                return Ok(new {
                    success = true,
                    message = "Data berhasil disimpan"
                });
            }

            return BadRequest(new {
                success = false,
                message = "Data gagal disimpan",
                data = req
            });
        }
    }
}