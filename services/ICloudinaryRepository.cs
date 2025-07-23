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
                Folder = folder// optional: folder di cloudinary
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
    }
}