using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mitraacd.Models;
using mitraacd.Services;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json; // ✅ untuk JsonSerializer
using Microsoft.Extensions.Logging; // ✅ untuk ILogger

using Google.Apis.Auth;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace mitraacd.api
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Account")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAccountRepository _repo;
        private readonly ILogger<AuthController> _logger; // ✅ tambahkan logger

        public AuthController(
            IConfiguration configuration, 
            IHttpClientFactory httpClientFactory,
            IAccountRepository repo,
            ILogger<AuthController> logger // ✅ inject logger
        )
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _repo = repo;
            _logger = logger;
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.IdToken))
            {
                return BadRequest("ID Token tidak boleh kosong.");
            }

            try
            {
                var googleClientId = _configuration["Authentication:Google:ClientId"];

                // Validasi ID Token dari Google
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    request.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { googleClientId }
                    });

                // Validasi email
                if (payload.EmailVerified != true)
                {
                    return Unauthorized("Email Google belum diverifikasi.");
                }

                // Validasi issuer
                var validIssuers = new[] { "https://accounts.google.com", "accounts.google.com" };
                if (!validIssuers.Any(i => string.Equals(i, payload.Issuer, StringComparison.OrdinalIgnoreCase)))
                {
                    return Unauthorized("Issuer Google tidak valid.");
                }

                // Ambil data user dari payload
                var userId = payload.Subject;
                var userEmail = payload.Email;
                var userName = payload.GivenName ?? payload.Name ?? userEmail; // fallback
                var userPicture = payload.Picture ?? string.Empty;

                // Cek user di DB
                var data = await _repo.CheckUser(userEmail);
                if (data == null)
                {
                    var newUser = new GoogleNewUser
                    {
                        UserIdGoogle = userId,
                        Name = userName,
                        Email = userEmail,
                        Picture = userPicture
                    };

                    var id = await _repo.CreateNewUserGoogle(newUser);
                    userId = id.ToString();
                }
                else
                {
                    userId = data.id;
                }

                // Buat claims
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Email, userEmail),
                    new Claim("picture", userPicture)
                };

                // Generate JWT
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Ambil expired dari config dengan fallback default (60 menit)
                double expiresInMinutes = 60;
                double.TryParse(_configuration["Jwt:ExpiresInMinutes"], out expiresInMinutes);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                    signingCredentials: creds
                );

                var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                // Ambil data user dari DB lagi
                _logger.LogInformation("Ambil data account untuk UserId={UserId}", userId);
                var user = await _repo.GetData_account(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User data tidak ditemukan di DB untuk UserId={UserId}", userId);
                    return StatusCode(500, "User data tidak ditemukan setelah login.");
                }

                var userJson = JsonSerializer.Serialize((object)user);
                _logger.LogInformation("User data ditemukan: {User}", userJson);

                return Ok(new
                {
                    token = jwt,
                    user = new
                    {
                        id = userId,
                        name = userName,
                        fullname = user.nama_lengkap,
                        nohp = user.no_hp_whatapp,
                        email = userEmail,
                        picture = userPicture
                    }
                });
            }
            catch (InvalidJwtException ex)
            {
                return Unauthorized("ID Token tidak valid, " + ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Terjadi kesalahan pada server, " + ex.Message);
            }
        }

        [HttpGet("GetData_account")]
        public async Task<ActionResult<dynamic>> GetData_account(string id)
        {
            var res = await _repo.GetData_account(id);
            return Ok(res);
        }

        [HttpPost("CheckNamaLengkapDanPanggilan")]
        public async Task<ActionResult<dynamic>> CheckNamaLengkapDanPanggilan([FromBody] ReqCheckNama req)
        {
            var res = await _repo.CheckNama(req);
            if(res)
            {
                return Ok(new { message = "Nama yang diinput sudah pernah terdaftar", data = req });
                
            }
            
            return Ok(res);
        }

        [HttpPost("SimpanBiodataMitra")]
        public async Task<ActionResult<dynamic>> SimpanBiodataMitra([FromBody] ReqSimpanBiodataMitra req)
        {
            var res = await _repo.SimpanBiodataMitra(req);
            if(res)
            {
                return Ok(new { message = "Data berhasil disimpan", data = req });   
            }
            return Ok(res);
        }
    }

}