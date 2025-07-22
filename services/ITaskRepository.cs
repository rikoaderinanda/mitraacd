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

    }

    public class TaskRepository : ITaskRepository
    {
        private readonly IDbConnection _db;

        public TaskRepository(IDbConnection db)
        {
            _db = db;
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

    }
}