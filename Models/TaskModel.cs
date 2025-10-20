using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace mitraacd.Models
{
    public class PengecekanAwal_Perbaikan_Dto
    {
        [JsonPropertyName("id_task")]
        public string Id_Task { get; set; }
        
        [JsonPropertyName("suhu")]
        public SatuanPengukuranDto Suhu { get; set; }

        [JsonPropertyName("tekanan")]
        public SatuanPengukuranDto Tekanan { get; set; }

        [JsonPropertyName("namePlate")]
        public NamePlateDto NamePlate { get; set; }

        [JsonPropertyName("waktu_submit")]
        public DateTime Waktu_Submit { get; set; }
    }

    public class SatuanPengukuranDto
    {
        [JsonPropertyName("foto")]
        public string Foto { get; set; }

        [JsonPropertyName("nilai")]
        public string Nilai { get; set; }
    }

    public class NamePlateDto
    {
        [JsonPropertyName("foto")]
        public string Foto { get; set; }

        [JsonPropertyName("brand")]
        public string Brand { get; set; }

        [JsonPropertyName("tipe")]
        public string Tipe { get; set; }
    }

    public class PengecekanLanjutanDto
    {
        public string Id_Task { get; set; }
        public List<KerusakanItemDto> List { get; set; } = new();
    }

    public class KerusakanItemDto
    {
        public string Id { get; set; }
        public string FotoKerusakan { get; set; }
        public string Deskripsi { get; set; }
        public string Rekomendasi { get; set; }
        public decimal Harga { get; set; }
    }

}