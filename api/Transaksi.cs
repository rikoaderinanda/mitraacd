using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mitraacd.Models;
using mitraacd.Services;

namespace mitraacd.api
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Task")]
    public class TransaksiController : ControllerBase
    {
        private readonly ITransaksiRepo _repo;
        public TransaksiController(ITransaksiRepo repo)
        {
            _repo = repo;
        }
        [HttpGet("GetTransaksiDetail")]
        public async Task<ActionResult<dynamic>> GetTransaksiDetail(string Id)
        {
            var res = await _repo.GetDetail(Id);
            return Ok(res);
        }

    }
}