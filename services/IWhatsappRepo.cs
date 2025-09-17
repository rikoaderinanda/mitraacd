using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using mitraacd.Models;
using Newtonsoft.Json;

namespace mitraacd.Services
{
    public interface IWhatsappRepo
    {
        Task<bool> SaveOtpAsync(string userId,string NoWa, string otpCode, string reswaId);
        Task<bool> CheckOTPAktif(CheckOTPAktifReq data);
    }

    public class WhatsappRepo : IWhatsappRepo
    {
        private readonly IDbConnection _db;

        public WhatsappRepo(IDbConnection db)
        {
            _db = db;
        }

        public async Task<bool> SaveOtpAsync(string userId,string NoWa, string otpCode, string reswaId)
        {
            var query = @"
                INSERT INTO otp_verification (user_id,no_wa,opt_code,reswa_id,expired_at,status)
                VALUES (@userId::bigint, @NoWa, @otpCode::bigint, @reswaId, @ExpiredAt,0)
                RETURNING id;
            ";

            var param = new
            {
                UserId = userId,
                NoWa = NoWa,
                otpCode = otpCode,
                reswaId = reswaId,
                ExpiredAt = DateTime.UtcNow.AddMinutes(5)
            };

            var result = await _db.ExecuteScalarAsync<long>(query, param);
            return result > 0;
        }

        public async Task<bool> CheckOTPAktif(CheckOTPAktifReq data)
        {
            var query = @"
                SELECT count(*)
                FROM otp_verification ov 
                WHERE 
                user_id = @user_id 
                AND no_wa = @no_wa
                AND expired_at > NOW()
            ";

            var param = new
            {
                user_id = data.user_id,
                no_wa = data.no_wa
            };

            var result = await _db.ExecuteScalarAsync<long>(query,param);
            return result > 0;
        }
        
    }


}