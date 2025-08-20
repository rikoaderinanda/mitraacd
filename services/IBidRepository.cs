using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using mitraacd.Models;
using Newtonsoft.Json;

namespace mitraacd.Services
{
    public interface IBidRepository
    {
        Task<IEnumerable<dynamic>> GetBidAsync();
        Task<int> TakeitAsync(TakeBidingModel Id);

    }

    public class BidRepository : IBidRepository
    {
        private readonly IDbConnection _db;

        public BidRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<dynamic>> GetBidAsync()
        {
            var sql = @"
                SELECT
                    lt.id,
                    lt.kategori_layanan,
                    lt.jenis_layanan,
                    lt.total_transaksi,
                    (cart_item::jsonb)->'Reguler' AS cart_items,
                    lt.jenis_properti::jsonb AS properti,
                    paket_member::jsonb AS paket
                FROM log_transaction lt 
                WHERE 
                status = 3
                ORDER BY id DESC;
            ";
            var result = await _db.QueryAsync<dynamic>(sql);

            return JsonColumnParser.ParseJsonColumns(result);
        }

        public async Task<IEnumerable<dynamic>> GetBidAsync1()
        {
            var finalResult = new List<dynamic>();
            string sql = "select*from lo";
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

        public async Task<int> TakeitAsync(TakeBidingModel dto)
        {
            
            var query = @"
                            select sp_bid_pemesanan(@Id,@MitraId);
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