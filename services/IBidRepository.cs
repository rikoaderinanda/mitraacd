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
        Task<IEnumerable<dynamic>> GetBidAsync(string Id);
        Task<bool> Takeit(TakeBidingModel data);

    }

    public class BidRepository : IBidRepository
    {
        private readonly IDbConnection _db;

        public BidRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<dynamic>> GetBidAsync(string _Id)
        {
            var sql = @"
                select a.* from
                (
                    select
                        id,
                        kategori_layanan ,jenis_layanan , 
                        (total_transaksi*70/100) fee_teknisi,
                        kunjungan,kontak_pelanggan,alamat_pelanggan,
                        alamat_pelanggan->>'koordinat' as tujuan,
                        (select alamat->>'koordinat'  from mitra_data where id = @Id) as origin,
                        ST_DistanceSphere(
                            ST_Point(
                                split_part(alamat_pelanggan->>'koordinat', ',', 2)::float,
                                split_part(alamat_pelanggan->>'koordinat', ',', 1)::float
                            ),
                            ST_Point(
                                split_part((select alamat->>'koordinat' from mitra_data where id = @Id), ',', 2)::float, 
                                split_part((select alamat->>'koordinat' from mitra_data where id = @Id), ',', 1)::float
                            )
                        ) as jarak_meter,
                        checkout_date,
                        jenis_properti
                    from log_transaction
                    where status = 3
                ) a
                WHERE jarak_meter <= (
                    SELECT (coverage_area->>'radius')::float * 1000
                    FROM mitra_data 
                    WHERE id = @Id
                )
                order by checkout_date asc
            ";

            var param = new
            {
                Id = long.Parse(_Id)
            };

            var result = await _db.QueryAsync<dynamic>(sql,param);

            return JsonColumnParser.ParseJsonColumns(result);
        }

        public async Task<bool> Takeit(TakeBidingModel data)
        {
            var query = @"
                update log_transaction lt 
                set
                    mitra_bid_id =@MitraId,
                    bid_date = now(),
                    status = 4
                where lt.status = 3
                and id = @Id
                returning id;
            ";

            var param = new
            {
                Id = data.Id,
                MitraId = data.MitraId
            };

            var result = await _db.ExecuteScalarAsync<int?>(query, param);

            return result.HasValue && result.Value > 0;
        }

    }
}