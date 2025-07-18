using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using mitraacd.Models;

namespace mitraacd.Services
{
    public interface IBidRepository
    {
        Task<IEnumerable<dynamic>> GetBidAsync();

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
            var sql = "SELECT * FROM pemesanan";
            return await _db.QueryAsync<dynamic>(sql);
        }
    }
}