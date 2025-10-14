using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mitraacd.Models
{
    public class GeneralRequest
    {
        public int Id { get; set; }
    }

    public class TakeBidingModel
    {
        public int Id { get; set; }
        public int MitraId { get; set; }
    }

    public class BerangkatKelokasiModel
    {
        public int Id { get; set; }
        public int MitraId { get; set; }
    }

    public class SampaiDiLokasiModel
    {
        public int Id { get; set; }
        public int MitraId { get; set; }
    }

    public class ImageUploadResultDto
    {
        public string IdTask { get; set; }
        public string Url { get; set; }
        public string PublicId { get; set; }
        public string Name { get; set; }
        public int idx { get; set; }
    }

    public class PhotoBeforeUploadDto
    {
        public List<IFormFile> ImageFiles { get; set; }
        public string IdTask { get; set; }
        public string pengukuran_awal {get;set;}
    }

    public class UploadSelfiePhotoDto
    {
        public IFormFile File { get; set; }
        public string Id { get; set; }
    }

    public class UpdateTask_PengukuranAwalDTO
    {
        public string IdTask { get; set; }
        public string pengukuran_awal { get; set; }
        public List<ImageUploadResultDto> imageResults { get; set; }
    }

    public class PhotoPekerjaanUploadDto
    {
        public string IdTask { get; set; }
        public List<IFormFile> ImageFiles { get; set; }
    }

    public class UpdateTask_PengerjaanDTO
    {
        public string IdTask { get; set; }
        public List<ImageUploadResultDto> imageResults { get; set; }
    }

    public class Photo_QA_UploadDto
    {
        public string IdTask { get; set; }
        public string pengukuran_akhir {get;set;}
        public List<IFormFile> ImageFiles { get; set; }
    }

    public class UpdateTask_QADTO
    {
        public string IdTask { get; set; }
        public string pengukuran_akhir { get; set; }
        public List<ImageUploadResultDto> imageResults { get; set; }
    }
}