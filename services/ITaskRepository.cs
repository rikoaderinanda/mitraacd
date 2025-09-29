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
        
        Task<dynamic> CheckQrCodeUnit(string decodedText);

        
        //versi 1.0
        Task<IEnumerable<dynamic>> GetTask(string Id, string FilterHari);
        Task<bool> Berangkat(BerangkatKelokasiModel data);
        Task<bool> SampaiLokasi(SampaiDiLokasiModel idtask);
        Task<bool> UpdateTask_PengukuranAwal(UpdateTask_PengukuranAwalDTO dt);
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

        public async Task<bool> UpdateTask_PengukuranAwal(UpdateTask_PengukuranAwalDTO dt)
        {
            var query = @"
                update log_transaction 
                set 
                    status = 7,
                    pengukuran_awal = @Pengukuran_awal::jsonb,
                    pengukuran_awal_datetime = now(),
                    img_pengukuran_awal = @Img_pengukuran_awal::jsonb
                where id = @Id
                returning id;
            ";

            var param = new
            {
                Id = long.Parse(dt.IdTask),
                Pengukuran_awal = dt.pengukuran_awal,
                img_pengukuran_awal = JsonConvert.SerializeObject(dt.imageResults)
            };

            var result = await _db.ExecuteScalarAsync<int?>(query, param);

            return result.HasValue && result.Value > 0;
        }

        public async Task<bool> SampaiLokasi(SampaiDiLokasiModel data)
        {
            var query = @"
                update log_transaction 
                    set status = 6,
                    sampai_dilokasi_date = now()
                where 
                id = @id
                and mitra_bid_id = @MitraId
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

        public async Task<bool> Berangkat(BerangkatKelokasiModel data)
        {
            var query = @"
                update log_transaction
                set 
                    berangkat_kelokasi_date = now(),
                    status = 5
                where 
                status = 4
                and id = @Id
                and mitra_bid_id = @MitraId
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

        public async Task<IEnumerable<dynamic>> GetTask(string Id, string FilterHari)
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
                        bid_date,
                        jenis_properti,
                        kunjungan->> 'Member' as mbr,
                        kunjungan->> 'Reguler' as reg,
                        to_timestamp(
                            (kunjungan->'Reguler'->>'tanggal') || ' ' || (kunjungan->'Reguler'->>'jam'),
                            'YYYY-MM-DD HH24:MI'
                        ) as waktu_kunjungan,
                        status
                    from log_transaction
                    where status > 3 and mitra_bid_id = @Id
                ) a
                WHERE 
                jarak_meter <= (
                    SELECT (coverage_area->>'radius')::float * 1000
                    FROM mitra_data 
                    WHERE id = @Id
                )
                AND (
                    (@FilterHari = 0 AND date(waktu_kunjungan) = current_date)
                    OR (@FilterHari = 1 AND date(waktu_kunjungan) = current_date + 1)
                    OR (@FilterHari = 2 AND date(waktu_kunjungan) > current_date + 1)
                )
                order by  waktu_kunjungan asc
            ";

            var param = new
            {
                Id = long.Parse(Id),
                FilterHari = long.Parse(FilterHari)
            };

            var result = await _db.QueryAsync<dynamic>(sql,param);

            return JsonColumnParser.ParseJsonColumns(result);
        }

        public async Task<IEnumerable<dynamic>> GetTaskAsync(int Id, int hari)
        {
            var finalResult = new List<dynamic>();
            string sql = "select order_json::text from get_task(" + Id + "," + hari + ")";
            var result = await _db.QueryAsync<string>(sql);
            
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

        

        public async Task<dynamic?> CheckQrCodeUnit(string decodedText)
        {
            string sql = @"
                SELECT to_jsonb(oi)
                FROM perangkat_pelanggan oi
                WHERE oi.qr_code = @code;
            ";

            var result = await _db.QueryFirstOrDefaultAsync<string>(sql, new { code = decodedText });

            if (string.IsNullOrWhiteSpace(result))
                return null;

            dynamic obj = JsonConvert.DeserializeObject<dynamic>(result);
            return obj;
        }

    }
}