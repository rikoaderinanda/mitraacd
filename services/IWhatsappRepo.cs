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
        Task<bool> SaveOtpAsync(string userId, string otpCode, DateTime expiredAt);
    }

    public class WhatsappRepo : IWhatsappRepo
    {
        private readonly IDbConnection _db;

        public WhatsappRepo(IDbConnection db)
        {
            _db = db;
        }

        public async Task<bool> SaveOtpAsync(string userId, string otpCode, DateTime expiredAt)
        {
            var query = @"
                INSERT INTO otp_verification (UserId, OtpCode, ExpiredAt)
                VALUES (@UserId, @OtpCode, @ExpiredAt);
                SELECT CAST(SCOPE_IDENTITY() as int);
            ";

            var param = new
            {
                UserId = userId,
                OtpCode = otpCode,
                ExpiredAt = expiredAt
            };

            var result = await _db.ExecuteScalarAsync<int>(query, param);
            return result > 0;
        }
    }


}