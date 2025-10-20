using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using mitraacd.Models;
using Newtonsoft.Json;


namespace mitraacd.Services
{
    public interface ICloudinaryRepository
    {
        Task<(string url, string publicId)> UploadImageAsync(IFormFile file,string folder);
        Task<bool> DeleteImageAsync(string publicId);
        Task<string> UploadBase64ImageAsync(string base64String, string folderName);
        Task<bool> DeleteImageByUrlAsync(string imageUrl);
    }

    public class CloudinaryRepository : ICloudinaryRepository
    {
        private readonly IDbConnection _db;
        private readonly Cloudinary _cloudinary;

        public CloudinaryRepository(IDbConnection db,IConfiguration config)
        {
            _db = db;
            var settings = config.GetSection("CloudinarySettings");
            var account = new Account(
                settings["CloudName"],
                settings["ApiKey"],
                settings["ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<(string url, string publicId)> UploadImageAsync(IFormFile file, string folder)
        {

            if (file == null || file.Length == 0)
                return (null,null);


            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,// optional: folder di cloudinary
                Transformation = new Transformation().Width(1280).Height(720).Quality(50).Crop("limit")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return (result.SecureUrl?.ToString(), result.PublicId);
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok";
        }

        public async Task<string> UploadBase64ImageAsync(string base64String, string folderName)
        {
            if (string.IsNullOrWhiteSpace(base64String))
                throw new Exception("Base64 kosong atau tidak valid");

            // Hapus prefix data URL agar bisa dikonversi
            string cleanedBase64 = base64String
                .Replace("data:image/png;base64,", "")
                .Replace("data:image/jpeg;base64,", "")
                .Replace("data:image/jpg;base64,", "")
                .Trim();

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(cleanedBase64);
            }
            catch
            {
                throw new Exception("Format base64 tidak valid");
            }

            using var stream = new MemoryStream(bytes);
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription($"{Guid.NewGuid()}.png", stream),
                Folder = folderName,
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception("Upload ke Cloudinary gagal");

            return result.SecureUrl?.ToString();
        }

        /// <summary>
        /// Menghapus gambar di Cloudinary berdasarkan URL-nya
        /// </summary>
        public async Task<bool> DeleteImageByUrlAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            try
            {
                // Ambil public_id dari URL, contoh:
                // https://res.cloudinary.com/demo/image/upload/v12345/Perbaikan/pengecekan_awal/suhu/abcd1234.jpg
                // public_id = Perbaikan/pengecekan_awal/suhu/abcd1234
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.Split('/');
                var folderAndName = string.Join('/', segments[segments.Length - 4], segments[segments.Length - 3], segments[segments.Length - 2]);
                var fileName = Path.GetFileNameWithoutExtension(segments[^1]);

                // gabungkan folder dan nama file
                var publicId = $"{folderAndName}/{fileName}";

                var deletionParams = new DeletionParams(publicId);
                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                return deletionResult.Result == "ok";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Gagal menghapus gambar Cloudinary: {ex.Message}");
                return false;
            }
        }

    }
}