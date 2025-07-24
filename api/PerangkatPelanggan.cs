using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mitraacd.Models;
using mitraacd.Services;

namespace mitraacd.api
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerangkatPelangganController : ControllerBase
    {
        private readonly IPerangkatPelangganRepository _repo;

        public PerangkatPelangganController(IPerangkatPelangganRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("GetPhoto")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetPhoto(string Id)
        {
            var res = await _repo.GetPhotoAsync(Id);
            return Ok(res);
        }

        [HttpGet("GetHistoryMaintenance")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetHistoryMaintenance(string Id)
        {
            var res = await _repo.GetHistoryMaintenance(Id);
            return Ok(res);
        }

    }
}