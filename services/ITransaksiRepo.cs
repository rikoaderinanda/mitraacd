using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using mitraacd.Models;
using Newtonsoft.Json;

namespace mitraacd.Services
{
    public interface ITransaksiRepo
    {
        Task<dynamic?> GetDetail(string Id);        
    }
    public class TransaksiRepo : ITransaksiRepo
    {
        private readonly IDbConnection _db;
        private readonly IDbConnectionFactory _connectionFactory;

        public TransaksiRepo(IDbConnection db, IDbConnectionFactory connectionFactory)
        {
            _db = db;
            _connectionFactory = connectionFactory;
        }

        public async Task<dynamic?> GetDetail(string Id)
        {
            var sql = @"
                select
                    id,
                    create_by_id_user as id_pelanggan,    
                    kategori_layanan,
                    jenis_layanan,
                    (
                        total_transaksi * (
                            select value_number 
                            from app_settings t 
                            where key_name = 'teknisi_fee_percentage'
                            limit 1
                        ) / 100
                    ) as fee_teknisi,
                    kunjungan,
                    kontak_pelanggan,
                    alamat_pelanggan,
                    alamat_pelanggan->>'koordinat' as tujuan,
                    (select alamat->>'koordinat' from mitra_data where id = a.mitra_bid_id) as origin,
                    ST_DistanceSphere(
                        ST_Point(
                            split_part(alamat_pelanggan->>'koordinat', ',', 2)::float,
                            split_part(alamat_pelanggan->>'koordinat', ',', 1)::float
                        ),
                        ST_Point(
                            split_part((select alamat->>'koordinat' from mitra_data where id = a.mitra_bid_id), ',', 2)::float, 
                            split_part((select alamat->>'koordinat' from mitra_data where id = a.mitra_bid_id), ',', 1)::float
                        )
                    ) as jarak_meter,
                    bid_date,
                    jenis_properti,
                    cart_item,
                    keluhan_perbaikan,
                    kunjungan->> 'Member' as mbr,
                    kunjungan->> 'Reguler' as reg,
                    to_timestamp(
                        (kunjungan->'Reguler'->>'tanggal') || ' ' || (kunjungan->'Reguler'->>'jam'),
                        'YYYY-MM-DD HH24:MI'
                    ) as waktu_kunjungan,
                    status
                from log_transaction a
                where a.status > 3 and a.id = @Id
                limit 1;
            ";

            // Validasi input ID agar tidak error kalau kosong atau bukan angka
            if (!long.TryParse(Id, out long parsedId))
                return null;

            var param = new { Id = parsedId };

            var result = await _db.QueryFirstOrDefaultAsync<dynamic>(sql, param);

            if (result == null)
                return null;

            // Parsing JSON column jika diperlukan
            var parsed = JsonColumnParser.ParseJsonColumns(new[] { result });

            return parsed.FirstOrDefault();
        }
    }

}