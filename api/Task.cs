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
        public async Task<ActionResult<IEnumerable<dynamic>>> GetTask(int Id, int Hari)
        {
            var res = await _taskRepository.GetTaskAsync(Id, Hari);
            return Ok(res);
        }

        [HttpPost("BerangkatKelokasi")]
        public async Task<IActionResult> BerangkatKelokasi([FromBody] BerangkatKelokasiModel dto)
        {
            if (dto == null || dto.Id == null)
                return BadRequest(new { message = "Data tidak tersedia" });
            try
            {
                var res = await _taskRepository.BerangkatKelokasiAsync(dto);
                return Ok(new { message = "Task berhasil diupdate", id = res });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan Biding" });
            }
        }

        [HttpPost("SampaiDiLokasi")]
        public async Task<IActionResult> SampaiDiLokasi([FromBody] SampaiDiLokasiModel dto)
        {
            if (dto == null || dto.Id == null)
                return BadRequest(new { message = "Data tidak tersedia" });
            try
            {
                var res = await _taskRepository.SampaiDiLokasiAsync(dto);
                return Ok(new { message = "Task berhasil diupdate", id = res });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan Biding" });
            }
        }

        [HttpGet("CheckPhotoSebelumTask")]
        public async Task<ActionResult<IEnumerable<dynamic>>> CheckPhotoSebelumTask(string Id)
        {
            var res = await _taskRepository.CheckPhotoSebelumTask(Id);
            return Ok(res);
        }

    }
}