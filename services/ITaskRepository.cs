using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using mitraacd.Models;
using Newtonsoft.Json;

namespace mitraacd.Services
{
    public interface ITaskRepository
    {
        Task<IEnumerable<dynamic>> GetTaskAsync(int Id, int hari);
        Task<int> BerangkatKelokasiAsync(BerangkatKelokasiModel Id);
        Task<int> SampaiDiLokasiAsync(SampaiDiLokasiModel Id);
        Task<bool> SimpanUrlFotoSebelumTask(ImageUploadResultDto dto);
        Task<IEnumerable<dynamic>> CheckPhotoSebelumTask(string IdTask);
        Task<bool> DeletePhotoSebelumTaskAsync(string publicId);
        Task<bool> UpdateStatusTaskAsync(string idtask);

    }

    public class TaskRepository : ITaskRepository
    {
        private readonly IDbConnection _db;
        private readonly ICloudinaryRepository _cloudinaryService;

        public TaskRepository(IDbConnection db, ICloudinaryRepository cloudinaryService)
        {
            _db = db;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<IEnumerable<dynamic>> GetTaskAsync(int Id, int hari)
        {
            var finalResult = new List<dynamic>();
            string sql = "select order_json::text from get_task(" + Id + "," + hari + ")";
            var result = await _db.QueryAsync<string>(sql);
            // Console.WriteLine(result.First().GetType().Name);
            foreach (var jsonString in result)
            {
                // Console.WriteLine("Raw JSON: " + jsonString);
                try
                {
                    dynamic? obj = JsonConvert.DeserializeObject<dynamic>(jsonString);
                    if (obj is not null)
                    {
                        finalResult.Add(obj);
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("Gagal parsing JSON: " + ex.Message);
                }
            }
            return finalResult;
        }

        public async Task<int> BerangkatKelokasiAsync(BerangkatKelokasiModel dto)
        {

            var query = @"
                            select sp_BerangkatKelokasi(@Id,@MitraId);
                        ";

            var param = new
            {
                Id = dto.Id,
                MitraId = dto.MitraId
            };

            int res = await _db.ExecuteScalarAsync<int>(query, param);
            return res;
        }

        public async Task<int> SampaiDiLokasiAsync(SampaiDiLokasiModel dto)
        {

            var query = @"
                            select sp_sampaidilokasi(@Id,@MitraId);
                        ";

            var param = new
            {
                Id = dto.Id,
                MitraId = dto.MitraId
            };

            int res = await _db.ExecuteScalarAsync<int>(query, param);
            return res;
        }

        public async Task<bool> SimpanUrlFotoSebelumTask(ImageUploadResultDto dto){
            var query = @"
                            select sp_simpan_url_foto_sebelum_task(@Id,@PublicId,@Url);
                        ";

            var param = new
            {
                Id = int.Parse(dto.IdTask),
                PublicId = dto.PublicId,
                Url = dto.Url,
            };

            var res = await _db.ExecuteScalarAsync<int>(query, param);
            return res!= null;
        }

        public async Task<IEnumerable<dynamic>> CheckPhotoSebelumTask(string IdTask)
        {   
            var finalResult = new List<dynamic>();
            string sql = "select data_json::text from sp_check_photo_sebelum_task(" + IdTask + ")";
            var result = await _db.QueryAsync<string>(sql);
            foreach (var jsonString in result)
            {
                try
                {
                    dynamic? obj = JsonConvert.DeserializeObject<dynamic>(jsonString);
                    if (obj is not null)
                    {
                        finalResult.Add(obj);
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("Gagal parsing JSON: " + ex.Message);
                }
            }
            return finalResult;
        }

        public async Task<bool> DeletePhotoSebelumTaskAsync(string idtask)
        {
            var sql = @"
                DELETE FROM photo_before_task
                WHERE id_task = @id
            ";

            var result = await _db.ExecuteAsync(sql, new {id = int.Parse(idtask)});
            return result > 0; // true jika ada baris yang dihapus
        }

        public async Task<bool> UpdateStatusTaskAsync(string idtask)
        {
            var sql = @"
                update pemesanan 
                    set status_order = 7
                where id = @id
            ";

            var result = await _db.ExecuteAsync(sql, new {id = int.Parse(idtask)});
            return result > 0; // true jika ada baris yang dihapus
        }

    }
}