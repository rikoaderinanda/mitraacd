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
    }
}