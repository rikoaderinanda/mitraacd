using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mitraacd.Models;
using mitraacd.Services;
using Newtonsoft.Json;

namespace mitraacd.api
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Task")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ICloudinaryRepository _cloudinaryRepo;

        public TaskController(ITaskRepository taskRepository, ICloudinaryRepository cloudinaryRepo)
        {
            _taskRepository = taskRepository;
            _cloudinaryRepo = cloudinaryRepo;
        }

        [HttpGet("GetTask")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetTask(string Id, string hari, string status)
        {
            var res = await _taskRepository.GetTask(Id, hari, status);
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
                return Ok(new
                {
                    success = true,
                    message = "Data berhasil disimpan"
                });
            }

            return BadRequest(new
            {
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
                return Ok(new
                {
                    success = true,
                    message = "Data berhasil disimpan"
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "Data gagal disimpan",
                data = req
            });
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

        [HttpGet("CheckPhotoSebelumTask")]
        public async Task<ActionResult<IEnumerable<dynamic>>> CheckPhotoSebelumTask(string Id)
        {
            var res = await _taskRepository.CheckPhotoSebelumTask(Id);
            return Ok(res);
        }

        [HttpPost("SubmitPengecekanAwal_Perbaikan")]
        public async Task<IActionResult> SubmitPengecekanAwal_Perbaikan([FromBody] PengecekanAwal_Perbaikan_Dto data)
        {
            if (data == null)
                return BadRequest(new { success = false, message = "Data tidak valid." });

            try
            {
                // --- Validasi wajib isi ---
                if (string.IsNullOrEmpty(data.Suhu?.Foto) ||
                    string.IsNullOrEmpty(data.Tekanan?.Foto) ||
                    string.IsNullOrEmpty(data.NamePlate?.Foto))
                {
                    return BadRequest(new { success = false, message = "Semua foto wajib diisi." });
                }

                // --- Upload ke Cloudinary ---
                var suhuUrl = await _cloudinaryRepo.UploadBase64ImageAsync(data.Suhu.Foto, $"Perbaikan/pengecekan_awal/{data.Id_Task}/suhu");
                var tekananUrl = await _cloudinaryRepo.UploadBase64ImageAsync(data.Tekanan.Foto, $"Perbaikan/pengecekan_awal/{data.Id_Task}/tekanan");
                var nameplateUrl = await _cloudinaryRepo.UploadBase64ImageAsync(data.NamePlate.Foto, $"Perbaikan/pengecekan_awal/{data.Id_Task}/nameplate");

                // --- Validasi hasil upload ---
                if (string.IsNullOrEmpty(suhuUrl) || string.IsNullOrEmpty(tekananUrl) || string.IsNullOrEmpty(nameplateUrl))
                {
                    return StatusCode(500, new { success = false, message = "Gagal upload satu atau lebih foto ke Cloudinary." });
                }

                // --- Buat payload JSON ---
                var payload = new
                {
                    Suhu = data.Suhu.Nilai,
                    Tekanan = data.Tekanan.Nilai,
                    Brand = data.NamePlate.Brand,
                    Tipe = data.NamePlate.Tipe,
                    FotoSuhu = suhuUrl,
                    FotoTekanan = tekananUrl,
                    FotoNamePlate = nameplateUrl,
                    data.Waktu_Submit
                };

                var payloadJson = JsonConvert.SerializeObject(payload);

                // --- Simpan ke database ---
                var saveResult = await _taskRepository.UpdateTask_PengukuranAwal_Perbaikan(data.Id_Task, payloadJson);

                if (!saveResult)
                {
                    // rollback (hapus foto yg sudah diupload)
                    await _cloudinaryRepo.DeleteImageByUrlAsync(suhuUrl);
                    await _cloudinaryRepo.DeleteImageByUrlAsync(tekananUrl);
                    await _cloudinaryRepo.DeleteImageByUrlAsync(nameplateUrl);

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Gagal menyimpan data ke database. Semua foto telah dihapus dari Cloudinary."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "✅ Data pengecekan awal berhasil disimpan.",
                    urls = new { suhu = suhuUrl, tekanan = tekananUrl, namePlate = nameplateUrl },
                    data = payload
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Terjadi kesalahan saat upload atau simpan data.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("SubmitPengecekanLanjutan_Perbaikan")]
        public async Task<IActionResult> SubmitPengecekanLanjutan_Perbaikan([FromBody] PengecekanLanjutanDto data)
        {
            if (data == null || data.List == null || !data.List.Any())
                return BadRequest(new { success = false, message = "Data tidak valid." });

            try
            {
                var uploadedList = new List<object>();

                foreach (var item in data.List)
                {
                    // Validasi dasar
                    if (string.IsNullOrEmpty(item.FotoKerusakan))
                        return BadRequest(new { success = false, message = $"Foto pada ID {item.Id} kosong." });

                    // Upload foto ke Cloudinary
                    var fotoUrl = await _cloudinaryRepo.UploadBase64ImageAsync(
                        item.FotoKerusakan, $"Perbaikan/pengecekan_lanjutan/{data.Id_Task}");

                    uploadedList.Add(new
                    {
                        item.Id,
                        item.Deskripsi,
                        item.Rekomendasi,
                        item.Harga,
                        FotoUrl = fotoUrl
                    });
                }

                // Simpan ke database (contoh pseudo)
                var payload = new
                {
                    IdTask = data.Id_Task,
                    KerusakanList = uploadedList,
                    TanggalInput = DateTime.Now
                };

                var saveResult = await _taskRepository.UpdateTask_PengecekanLanjutan(data.Id_Task, payload);

                return Ok(new
                {
                    success = true,
                    message = "✅ Data pengecekan lanjutan berhasil disimpan.",
                    data = payload
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Terjadi kesalahan saat upload ke Cloudinary.",
                    error = ex.Message
                });
            }
        }


    }
}