using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using mitraacd.Models;
using Newtonsoft.Json;

namespace mitraacd.Services
{
    public interface IPerangkatPelangganRepository
    {
        Task<IEnumerable<dynamic>> GetPhotoAsync(string Id);
        Task<IEnumerable<dynamic>> GetHistoryMaintenance(string IdPerangkat);
        
    }

    public class PerangkatPelangganRepository : IPerangkatPelangganRepository
    {
        private readonly IDbConnection _db;

        public PerangkatPelangganRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<dynamic>> GetPhotoAsync(string Id)
        {
            var finalResult = new List<dynamic>();
            string sql = @"
                SELECT to_jsonb(oi)
                FROM photo_perangkat oi
                WHERE oi.id = @id;
            ";
            var result = await _db.QueryAsync<string>(sql,new { id = int.Parse(Id)});
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
        public async Task<IEnumerable<dynamic>> GetHistoryMaintenance(string IdPerangkat)
        {
            var finalResult = new List<dynamic>();
            string sql = @"
                SELECT to_jsonb(oi)
                FROM history_perangkat oi
                WHERE oi.id_perangkat = @IdPerangkat
                order by oi.id asc;
            ";
            var result = await _db.QueryAsync<string>(sql,new { IdPerangkat = int.Parse(IdPerangkat)});
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
    }
}