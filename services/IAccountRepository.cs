using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using mitraacd.Models;
using Newtonsoft.Json;

namespace mitraacd.Services
{
    public interface IAccountRepository
    {
        Task<dynamic?> CheckUser(string email);
        Task<long?> CreateNewUserGoogle(GoogleNewUser user);
        Task<IEnumerable<dynamic?>> GetListKontakAsync(string IdUser);
        Task<bool> SimpanKontakAsync(Kontak data);
        Task<bool> DeleteKontak(string _id);
        Task<IEnumerable<dynamic?>> GetAlamatAsync(string IdUser);
        Task<bool> DeleteAlanat(string _id);
        Task<bool> SimpanAlamatAsync(AlamatPelanggan data);
        Task<bool> CheckLogTrxDgnAlamat(string _id);
        Task<bool> UpdateAccount(ReqUpdateAkun data);
        Task<dynamic> GetData_account(string id);
        
        Task<bool> CheckNama(ReqCheckNama data);
        Task<bool> SimpanBiodataMitra(ReqSimpanBiodataMitra data);
        
        Task<bool> CheckOTPValid(CheckOTPValidReq data);
        Task<bool> UpdateOTPStatus(CheckOTPValidReq data);
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly IDbConnection _db;

        public AccountRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<dynamic?> CheckUser(string _email)
        {
            string sql = @"
                SELECT to_jsonb(oi)
                FROM mitra_data oi
                WHERE oi.email = @email;
            ";

            var result = await _db.QueryFirstOrDefaultAsync<string>(sql, new { email = _email });

            if (string.IsNullOrWhiteSpace(result))
                return null;

            dynamic obj = JsonConvert.DeserializeObject<dynamic>(result);
            return obj;
        }

        public async Task<long?> CreateNewUserGoogle(GoogleNewUser user)
        {
            string sql = @"
                INSERT INTO mitra_data
                (
                    user_id_google,
                    username,
                    email,
                    photo
                )
                VALUES (
                    @user_id_google,
                    @username,
                    @email,
                    @photo
                )
                RETURNING id;
            ";

            var param = new {
                user_id_google = user.UserIdGoogle,
                username = user.Name,
                email = user.Email,      
                photo = user.Picture     
            };

            var id = await _db.QueryFirstOrDefaultAsync<long?>(sql, param);
            return id;
        }

        public async Task<IEnumerable<dynamic?>> GetListKontakAsync(string IdUser)
        {
            var sql = "SELECT * FROM kontak_pelanggan where user_id = @Id::bigint ORDER BY id asc";
            var param = new
            {
                Id = IdUser
            };

            return await _db.QueryAsync<dynamic>(sql, param);
        }

        public async Task<bool> SimpanKontakAsync(Kontak data)
        {
            try
            {
                var sql = @"
                    INSERT INTO public.kontak_pelanggan (user_id,nama_kontak,nomor_kontak)
                	VALUES (@_user_id,@_nama_kontak,@_nomor_kontak)
                    ON CONFLICT (user_id, nama_kontak, nomor_kontak) DO NOTHING;
                ";

                var param = new
                {
                    _user_id = data.UserId,
                    _nama_kontak = data.NamaKontak,
                    _nomor_kontak = data.NomorKontak
                };

                var affectedRows = await _db.ExecuteAsync(sql, param);
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                // Log exception jika kamu pakai logger
                // _logger.LogError(ex, "Error updating invoice status for ID: {Id}", id);
                return false; // bisa juga throw lagi kalau ingin controller yang menangani
            }
        }

        public async Task<bool> DeleteKontak(string _id)
        {
            var query = "DELETE FROM kontak_pelanggan WHERE id = @Id::bigint;";
            var result = await _db.ExecuteAsync(query, new { Id = _id });
            return result > 0;
        }

        public async Task<IEnumerable<dynamic?>> GetAlamatAsync(string IdUser)
        {
            var sql = "SELECT * FROM alamat_pelanggan where id_user = @Id::bigint ORDER BY id asc";
            var param = new
            {
                Id = IdUser
            };
            var result = await _db.QueryAsync<dynamic>(sql, param);
            return JsonColumnParser.ParseJsonColumns(result);
        }

        public async Task<bool> DeleteAlanat(string _id)
        {
            var query = "DELETE FROM alamat_pelanggan WHERE id = @Id::bigint;";
            var result = await _db.ExecuteAsync(query, new { Id = _id });
            return result > 0;
        }

        public async Task<bool> CheckLogTrxDgnAlamat(string _id)
        {
            var query = @"
                SELECT COUNT(1)
                FROM log_transaction
                WHERE (alamat_pelanggan::jsonb)->>'id' = @Id
                AND status <= 1;
            ";

            var result = await _db.ExecuteScalarAsync<int>(query, new { Id = _id });
            return result > 0;
        }

        public async Task<bool> SimpanAlamatAsync(AlamatPelanggan data)
        {
            try
            {
                var sql = @"
                    INSERT INTO alamat_pelanggan (
                        judul,
                        alamat,
                        provinsi_code,
                        provinsi_nama,
                        kota_code,
                        kota_nama,
                        kecamatan_code,
                        kecamatan_nama,
                        kelurahan_code,
                        kelurahan_nama,
                        id_user,
                        jenis_properti,
                        koordinat
                    ) VALUES (
                        @judul,
                        @alamat,
                        @provinsi_code,
                        @provinsi_nama,
                        @kota_code,
                        @kota_nama,
                        @kecamatan_code,
                        @kecamatan_nama,
                        @kelurahan_code,
                        @kelurahan_nama,
                        @id_user,
                        @jenis_properti::jsonb,
                        @koordinat
                    )
                    ON CONFLICT (id_user,judul, alamat) DO NOTHING
                    RETURNING id;
                ";

                var param = new
                {
                    judul = data.Judul?.Trim(),
                    alamat = data.Alamat?.Trim(),
                    provinsi_code = data.ProvinsiCode,
                    provinsi_nama = data.ProvinsiNama,
                    kota_code = data.KotaCode,
                    kota_nama = data.KotaNama,
                    kecamatan_code = data.KecamatanCode,
                    kecamatan_nama = data.KecamatanNama,
                    kelurahan_code = data.KelurahanCode,
                    kelurahan_nama = data.KelurahanNama,
                    id_user = data.IdUser,
                    jenis_properti = JsonConvert.SerializeObject(data.JenisProperti),
                    koordinat = data.Koordinat
                };

                // var affectedRows = await _db.ExecuteAsync(sql, param);
                // return affectedRows > 0;
                Console.WriteLine("SQL:");
                Console.WriteLine(sql);

                Console.WriteLine("Params:");
                foreach (var p in param.GetType().GetProperties())
                {
                    var val = p.GetValue(param, null);
                    Console.WriteLine($"{p.Name} = {val}");
                }

                var insertedId = await _db.ExecuteScalarAsync<int?>(sql, param);

                return insertedId.HasValue;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAccount(ReqUpdateAkun data)
        {
            var query = @"
                update mitra_data
                set 
                    nama_lengkap = @FullName,
                    no_hp_whatapp = @NoHpAkun
                where id = @Id
                RETURNING id;
            ";

            var param = new
            {
                Id = data.Id,
                FullName = data.FullName,
                NoHpAkun =  data.NoHpAkun
            };
            var result = await _db.ExecuteScalarAsync<int>(query, param);
            return result > 0;
        }

        public async Task<dynamic?> GetData_account(string id)
        {
            var sql = @"
                SELECT * FROM mitra_data 
                WHERE id = @Id::bigint;
            ";

            var param = new { Id = id };
            var result = await _db.QueryFirstOrDefaultAsync<dynamic>(sql, param);

            return JsonColumnParser.ParseJsonColumns(new[] { result }).FirstOrDefault();
        }

        public async Task<bool> CheckNama(ReqCheckNama data)
        {
            var query = @"
                select count(*) from mitra_data
                where 
                    (LOWER(nama_lengkap) = LOWER(@NamaLengkap) or LOWER(username) = LOWER(@NamaPanggilan))
                    and id not in (@Id);
            ";

            var param = new
            {
                NamaLengkap = data.NamaLengkap,
                NamaPanggilan = data.NamaPanggilan,
                Id = data.Id
            };
            var result = await _db.ExecuteScalarAsync<int>(query, param);
            return result > 0;
        }

        public async Task<bool> SimpanBiodataMitra(ReqSimpanBiodataMitra data)
        {
            var query = @"
                update mitra_data
                set 
                    nama_lengkap = @NamaLengkap,
                    username = @NamaPanggilan,
                    jenis_kelamin = @JenisKelamin,
                    tanggal_lahir = @TanggalLahir,
                    no_hp_whatapp = @NoWhatsapp,
                    nik = @Nik,
                    alamat = @Alamat::jsonb,
                    photo_selfie = @FotoSelfie::jsonb,
                    ktp = @FotoKTP::jsonb,
                    status_mitra = 1
                where id = @Id
                RETURNING id;";

            var param = new
            {
                Id = data.Id,
                NamaLengkap = data.NamaLengkap,
                NamaPanggilan = data.NamaPanggilan,
                JenisKelamin = data.JenisKelamin,
                TanggalLahir = data.TanggalLahir,
                NoWhatsapp = data.NoWhatsapp,
                Nik = data.Nik,
                Alamat = JsonConvert.SerializeObject(data.Alamat),
                FotoSelfie = JsonConvert.SerializeObject(data.FotoSelfie),
                FotoKTP = JsonConvert.SerializeObject(data.FotoKTP)
            };
            
            var result = await _db.ExecuteScalarAsync<int>(query, param);
            return result > 0;
        }

        public async Task<bool> CheckOTPValid(CheckOTPValidReq data)
        {
            var query = @"
                select count(*) from otp_verification
                where 
                    user_id = @user_id 
                    AND no_wa = @no_wa
                    AND expired_at > NOW()
                    and opt_code = @otp::bigint
                    and status = 0
                    ;
            ";

            var param = new
            {
                user_id = data.user_id,
                no_wa = data.no_wa,
                otp = data.otp
            };
            var result = await _db.ExecuteScalarAsync<int>(query, param);
            return result > 0;
        }

        public async Task<bool> UpdateOTPStatus(CheckOTPValidReq data)
        {
            var query = @"
                update 
                    otp_verification
                set 
                    status = 1,
                    verified_at = NOW()
                where 
                    user_id = @user_id::bigint
                    AND no_wa = @no_wa
                    and status = 0
                RETURNING id;
            ";

            var param = new
            {
                user_id = data.user_id,
                no_wa = data.no_wa
            };

            var result = await _db.ExecuteScalarAsync<int>(query, param);
            return result > 0;
        }

    }


}