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
    [ApiExplorerSettings(GroupName = "Cloudinary")]
    public class CloudinaryController : ControllerBase
    {
        private readonly ICloudinaryRepository _cloudinaryService;
        private readonly ITaskRepository _TaskService;
        private readonly IAccountRepository _AccountRepo;

        public CloudinaryController(ICloudinaryRepository cloudinaryService,ITaskRepository taskService,IAccountRepository AccountRepo)
        {
            _cloudinaryService = cloudinaryService;
            _TaskService = taskService;
            _AccountRepo = AccountRepo;
        }

        #region Task - Perbaikan AC

        #endregion

        [HttpPost("PhotoBeforeUpload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PhotoBeforeUpload([FromForm] PhotoBeforeUploadDto dto)
        {
            try
            {
                if (dto.ImageFiles  == null || !dto.ImageFiles .Any())
                    return BadRequest(new { message = "Tidak ada file" });
                if (dto.ImageFiles .Count > 3)
                    return BadRequest(new { message = "Maksimal 3 gambar" });
                
                dynamic data = await _TaskService.CheckPhotoSebelumTask(dto.IdTask);
                if(data != null){
                    foreach (var item in data)
                    {
                        var success = await _cloudinaryService.DeleteImageAsync(item.public_id.ToString());
                    }
                    await _TaskService.DeletePhotoSebelumTaskAsync(dto.IdTask);
                }

                var uploadTasks = dto.ImageFiles.Select(async (file, idx) =>
                {
                    if (file.Length > 5 * 1024 * 1024)
                        throw new Exception("File terlalu besar");

                    if (!file.ContentType.StartsWith("image/"))
                        throw new Exception("Hanya gambar yang diperbolehkan");

                    var (url, publicId) = await _cloudinaryService.UploadImageAsync(file, "UnitBeforeCheck");

                    string name_pic = idx switch
                    {
                        0 => "Suhu",
                        1 => "Tekanan",
                        2 => "Ampere",
                        _ => "Foto"
                    };

                    var result = new ImageUploadResultDto
                    {
                        IdTask = dto.IdTask,
                        Url = url,
                        PublicId = publicId,
                        Name = name_pic
                    };

                    var res = await _TaskService.SimpanUrlFotoSebelumTask(result);
                    if (!res)
                    {
                        await _cloudinaryService.DeleteImageAsync(result.PublicId);
                        throw new Exception("Gagal menyimpan ke database");
                    }

                    return result;
                });

                // Jalankan semua upload sekaligus
                var imageResults = (await Task.WhenAll(uploadTasks)).ToList();

                var up = new UpdateTask_PengukuranAwalDTO {
                    IdTask = dto.IdTask,
                    pengukuran_awal = dto.pengukuran_awal,
                    imageResults = imageResults
                };

                Console.WriteLine("Data UP: " + JsonConvert.SerializeObject(up));
                var resres = await _TaskService.UpdateTask_PengukuranAwal(up);
                return Ok(new { message = "Upload berhasil", data = up, success = resres });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan gambar, error:"+ex.Message});
            }
        }

        [HttpPost("PhotoPekerjaanUpload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PhotoPekerjaanUpload([FromForm] PhotoPekerjaanUploadDto dto)
        {
            try
            {
                if (dto.ImageFiles  == null || !dto.ImageFiles .Any())
                    return BadRequest(new { message = "Tidak ada file" });
                if (dto.ImageFiles .Count > 3)
                    return BadRequest(new { message = "Maksimal 3 gambar" });
                
                dynamic data = await _TaskService.CheckPhotoPengerjaanExistsTask(dto.IdTask);
                if(data != null){
                    foreach (var item in data)
                    {
                        var success = await _cloudinaryService.DeleteImageAsync(item.public_id.ToString());
                    }
                    await _TaskService.DeletePengerjaanTask(dto.IdTask);
                }

                int count = 0;
                var imageResults = new List<ImageUploadResultDto>();
                foreach (var file in dto.ImageFiles )
                {
                    
                    if (file.Length > 5 * 1024 * 1024)
                        return BadRequest("File terlalu besar");

                    if (!file.ContentType.StartsWith("image/"))
                        return BadRequest("Hanya gambar yang diperbolehkan");

                    var (url, publicId) = await _cloudinaryService.UploadImageAsync(file,"Doc_Pengerjaan");
                    if (!string.IsNullOrEmpty(url))
                    {
                        count++;
                        var _dt = new ImageUploadResultDto
                        {
                            IdTask = dto.IdTask,
                            Url = url,
                            PublicId = publicId,
                            idx = count
                        };

                        var res = await _TaskService.SimpanUrlFotoPengerjaanTask(_dt);
                        if(!res){
                            var del = await _cloudinaryService.DeleteImageAsync(_dt.PublicId);
                            return StatusCode(500, new { message = "Cloudinary upload failed" });    
                        }
                        imageResults.Add(_dt);
                    }
                    else
                    {
                        return StatusCode(500, new { message = "Cloudinary upload failed" });
                    }
                }
                
                var up = new UpdateTask_PengerjaanDTO {
                    IdTask = dto.IdTask,
                    imageResults = imageResults
                };
                var resres = await _TaskService.UpdateTask_Pengerjaan(up);
                return Ok(new { message = "Upload berhasil", data = up, success = resres });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan gambar, error:"+ex.Message});
            }
        }

        [HttpPost("PhotoPengukuran_QA")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PhotoPengukuran_QA([FromForm] Photo_QA_UploadDto dto)
        {
            try
            {
                if (dto.ImageFiles  == null || !dto.ImageFiles .Any())
                    return BadRequest(new { message = "Tidak ada file" });
                if (dto.ImageFiles .Count > 3)
                    return BadRequest(new { message = "Maksimal 3 gambar" });
                
                dynamic data = await _TaskService.CheckPhoto_QA_Task(dto.IdTask);
                if(data != null){
                    foreach (var item in data)
                    {
                        var success = await _cloudinaryService.DeleteImageAsync(item.public_id.ToString());
                    }
                    await _TaskService.DeletePhoto_QA(dto.IdTask);
                }

                var imageResults = new List<ImageUploadResultDto>();
                int idx = 0;
                foreach (var file in dto.ImageFiles )
                {
                    
                    if (file.Length > 5 * 1024 * 1024)
                        return BadRequest("File terlalu besar");

                    if (!file.ContentType.StartsWith("image/"))
                        return BadRequest("Hanya gambar yang diperbolehkan");

                    var (url, publicId) = await _cloudinaryService.UploadImageAsync(file,"Photo_QA_Task");
                    if (!string.IsNullOrEmpty(url))
                    {
                        string name_pic = "";
                        if(idx == 0){
                            name_pic ="Suhu";
                        }
                        else if(idx == 1){
                            name_pic ="Tekanan";
                        }
                        else if(idx == 2){
                            name_pic ="Ampere";
                        }

                        var _dt = new ImageUploadResultDto{
                            IdTask = dto.IdTask,
                            Url = url,
                            PublicId = publicId,
                            Name = name_pic
                        };
                        var res = await _TaskService.SimpanUrlFoto_QA(_dt);
                        if(!res){
                            var del = await _cloudinaryService.DeleteImageAsync(_dt.PublicId);
                            return StatusCode(500, new { message = "Cloudinary upload failed" });    
                        }
                        imageResults.Add(_dt);
                    }
                    else
                    {
                        return StatusCode(500, new { message = "Cloudinary upload failed" });
                    }
                    idx++;
                }
                var up = new UpdateTask_QADTO {
                    IdTask = dto.IdTask,
                    pengukuran_akhir = dto.pengukuran_akhir,
                    imageResults = imageResults
                };

                Console.WriteLine("Data UP: " + JsonConvert.SerializeObject(up));
                var resres = await _TaskService.UpdateTask_QA(up);
                return Ok(new { message = "Upload berhasil", data = up, success = resres });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan gambar, error:"+ex.Message});
            }
        }

        [HttpPost("deletePhotoBefore")]
        public async Task<IActionResult> deletePhotoBefore([FromForm] string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return BadRequest(new { message = "public_id kosong" });

            var success = await _cloudinaryService.DeleteImageAsync(publicId);

            if (!success)
                return StatusCode(500, new { message = "Gagal menghapus gambar" });

            return Ok(new { message = "Gambar berhasil dihapus" });
        }

        [HttpPost("UploadSelfiPhotoRegistrasi")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadSelfiPhotoRegistrasi([FromForm] UploadSelfiePhotoDto dto)
        {
            try
            {
                if (dto.File == null)
                    return BadRequest(new { message = "Tidak ada file" });

                if (dto.File.Length > 5 * 1024 * 1024)
                    return BadRequest("File terlalu besar");

                if (!dto.File.ContentType.StartsWith("image/"))
                    return BadRequest("Hanya gambar yang diperbolehkan");

                var (url, publicId) = await _cloudinaryService.UploadImageAsync(dto.File,"Account/Mitra/PHOTO");
                if (!string.IsNullOrEmpty(url))
                {
                    var dt = new ImageUploadResultDto{
                        IdTask = dto.Id,
                        Url = url,
                        PublicId = publicId
                    };
                    return Ok(new { message = "Upload berhasil", data = dt });
                }
                else
                {
                    return StatusCode(500, new { message = "Cloudinary upload failed" });
                }   
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan gambar, error:"+ex.Message});
            }
        }
        
        [HttpPost("UploadKTP")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadKTP([FromForm] UploadSelfiePhotoDto dto)
        {
            try
            {
                if (dto.File == null)
                    return BadRequest(new { message = "Tidak ada file" });

                if (dto.File.Length > 5 * 1024 * 1024)
                    return BadRequest("File terlalu besar");

                if (!dto.File.ContentType.StartsWith("image/"))
                    return BadRequest("Hanya gambar yang diperbolehkan");

                var (url, publicId) = await _cloudinaryService.UploadImageAsync(dto.File,"Account/Mitra/KTP");
                if (!string.IsNullOrEmpty(url))
                {
                    var dt = new ImageUploadResultDto{
                        IdTask = dto.Id,
                        Url = url,
                        PublicId = publicId
                    };
                    return Ok(new { message = "Upload berhasil", data = dt });
                }
                else
                {
                    return StatusCode(500, new { message = "Cloudinary upload failed" });
                }   
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan gambar, error:"+ex.Message});
            }
        }

        [HttpPost("deleteFile")]
        public async Task<IActionResult> deleteFile([FromForm] string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return BadRequest(new { message = "public_id kosong" });

            var success = await _cloudinaryService.DeleteImageAsync(publicId);

            if (!success)
                return StatusCode(500, new { message = "Gagal menghapus gambar" });

            return Ok(new { message = "Gambar berhasil dihapus" });
        }

    }
}