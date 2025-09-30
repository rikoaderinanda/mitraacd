using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mitraacd.Models;
using mitraacd.Services;

namespace mitraacd.api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;

        public TaskController(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        [HttpGet("GetTask")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetTask(string Id,string hari)
        {
            var res = await _taskRepository.GetTask(Id, hari);
            return Ok(res);
        }

        [HttpPost("BerangkatKelokasi")]
        public async Task<IActionResult> BerangkatKelokasi([FromBody] BerangkatKelokasiModel dto)
        {
            if (dto == null || dto.Id == null)
                return BadRequest(new { message = "Data tidak tersedia" });
            try
            {
                var res = await _taskRepository.Berangkat(dto);
                return Ok(new { message = "Task berhasil diupdate", id = res });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan Biding" });
            }
        }

        [HttpPost("Berangkat")]
        public async Task<IActionResult> Berangkat([FromBody] BerangkatKelokasiModel req)
        {
            var res = await _taskRepository.Berangkat(req);
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

        [HttpPost("SampaiDiLokasi")]
        public async Task<IActionResult> SampaiDiLokasi([FromBody] SampaiDiLokasiModel req)
        {
            var res = await _taskRepository.SampaiLokasi(req);
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

        [HttpGet("CheckPhotoSebelumTask")]
        public async Task<ActionResult<IEnumerable<dynamic>>> CheckPhotoSebelumTask(string Id)
        {
            var res = await _taskRepository.CheckPhotoSebelumTask(Id);
            return Ok(res);
        }

        [HttpGet("CheckQrCodeUnit")]
        public async Task<ActionResult<IEnumerable<dynamic>>> CheckQrCodeUnit(string decodedText)
        {
            var res = await _taskRepository.CheckQrCodeUnit(decodedText);
            return Ok(res);
        }

        [HttpGet("GetDataKonfirmasiPekerjaan")]
        public async Task<ActionResult<dynamic>> GetDataKonfirmasiPekerjaan(string Id)
        {
            var res = await _taskRepository.GetDataKonfirmasiPekerjaan(Id);
            return Ok(res);
        }

    }
}