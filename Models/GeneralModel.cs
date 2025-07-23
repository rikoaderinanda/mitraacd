using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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
        public string IdTask {get;set;}
        public string Url { get; set; }
        public string PublicId { get; set; }
    }
}