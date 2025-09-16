using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace mitraacd.Models
{
    public class ReqCheckNama
    {
        public long Id {get;set;}
        public string NamaLengkap { get; set; }
        public string NamaPanggilan { get; set; }
    }

    public class ReqUpdateAkun
    {
        public long Id { get; set; }
        public string FullName {get;set;}
        public string NoHpAkun {get;set;}
    } 

    public class ReqGetAkun
    {
        public string Id { get; set; }
    }

    public class Kontak
    {
        public int Id { get; set; }             // serial4 (auto-increment)
        public DateTime CreateAt { get; set; }  // timestamp
        public long UserId { get; set; }        // int8 (bigint)
        public string NamaKontak { get; set; }  // varchar
        public string NomorKontak { get; set; } // varchar
    }

    public class AlamatPelanggan
    {
        public long Id { get; set; }
        public object? JenisProperti { get; set; }
        public string Judul { get; set; }
        public string Alamat { get; set; }

        public string ProvinsiCode { get; set; }
        public string ProvinsiNama { get; set; }

        public string KotaCode { get; set; }
        public string KotaNama { get; set; }

        public string KecamatanCode { get; set; }
        public string KecamatanNama { get; set; }

        public string KelurahanCode { get; set; }
        public string KelurahanNama { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int IdUser { get; set; }

        public string Koordinat { get; set; }
    }

    public class ReqSimpanBiodataMitra
    {
        public int Id { get; set; }                // id registrasi
        public string NamaLengkap { get; set; }
        public string NamaPanggilan { get; set; }
        public string JenisKelamin { get; set; }
        public DateTime? TanggalLahir { get; set; } // bisa nullable
        
        public string NoWhatsapp { get; set; }
        public string Nik { get; set; }

        public object? Alamat { get; set; }
        public object? FotoSelfie { get; set; }
        public object? FotoKTP { get; set; }
    }

    public class SendMessageRequest
    {
        public string To { get; set; } // nomor WA dengan kode negara, ex: "6281234567890"
        public string Message { get; set; }
    }
}
