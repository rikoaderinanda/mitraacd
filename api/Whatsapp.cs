using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mitraacd.Services; // contoh repo kalau mau simpan OTP
using mitraacd.Models;


namespace mitraacd.api
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Whatsapp")]
    public class WhatsappController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWhatsappRepo _repo;
        private readonly IAccountRepository _repoAcc;
        private readonly ILogger<WhatsappController> _logger;

        public WhatsappController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IWhatsappRepo repo,
            ILogger<WhatsappController> logger
        )
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _repo = repo;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint verifikasi webhook (dari Meta Developer Dashboard)
        /// </summary>
        [HttpGet("webhook")]
        public IActionResult VerifyWebhook(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string verifyToken)
        {
            var expectedToken = _configuration["Whatsapp:VerifyToken"]; // simpan di appsettings.json
            if (mode == "subscribe" && verifyToken == expectedToken)
            {
                return Ok(challenge);
            }
            return Forbid();
        }

        /// <summary>
        /// Endpoint untuk menerima pesan dari user via WhatsApp
        /// </summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveMessage()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("Webhook POST dipanggil tapi body kosong!");
                return Ok();
            }
            
            _logger.LogInformation("Pesan masuk dari WhatsApp: {Body}", body);

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                // Ambil nomor pengirim & pesan user (hanya contoh, tergantung struktur JSON dari WA)
                var from = root
                    .GetProperty("entry")[0]
                    .GetProperty("changes")[0]
                    .GetProperty("value")
                    .GetProperty("messages")[0]
                    .GetProperty("from")
                    .GetString();

                var text = root
                    .GetProperty("entry")[0]
                    .GetProperty("changes")[0]
                    .GetProperty("value")
                    .GetProperty("messages")[0]
                    .GetProperty("text")
                    .GetProperty("body")
                    .GetString();

                if (!string.IsNullOrEmpty(text) && text.Trim().ToUpper() == "OTP")
                {
                    var otp = new Random().Next(100000, 999999).ToString();

                    // Simpan OTP ke repo (DB/Redis) dengan expiry
                    //await _repo.SaveOtpAsync(from, otp, DateTime.UtcNow.AddMinutes(5));

                    // Kirim OTP via WA
                    await SendWhatsappMessageAsync(from, $"Kode verifikasi Anda adalah: *{otp}* (berlaku 5 menit)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memproses pesan WhatsApp");
            }

            return Ok();
        }

        /// <summary>
        /// API untuk kirim pesan manual ke nomor WhatsApp
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Nomor tujuan dan pesan wajib diisi");

            try
            {
                var res = await SendWhatsappMessageAsync(request.To, request.Message);
                return Ok(new { success = true, to = request.To, message = request.Message, response = res });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal kirim pesan WhatsApp");
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// API untuk kirim pesan manual ke nomor WhatsApp
        /// </summary>
        [HttpPost("sendOTP")]
        public async Task<IActionResult> sendOTP([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Nomor tujuan dan pesan wajib diisi");
            try
            {
                var d = new CheckOTPAktifReq();
                d.user_id = request.UserId;
                d.no_wa = request.To;
                var cekOTPAktif = await _repo.CheckOTPAktif(d);
                
                if(cekOTPAktif){
                    return Ok(new {success = false, message = "OTP yang sebelumnya belum dipakai"});
                }
                
                var otp = new Random().Next(100000, 999999).ToString();
string otpMessage = 
$@"
🔐 *Kode Verifikasi Anda* 🔐
Kode OTP: 

*{otp}*

Berlaku selama 5 menit.
Jangan bagikan kode ini kepada siapa pun demi keamanan akun Anda.  
_AC Dikari Mitra - PT Dikari Tata Udara Indonesia._
";
                var res = await SendWhatsappMessageAsync(request.To, otpMessage);
                _logger.LogInformation("Balasan WA: {res}", res);
                using var doc = JsonDocument.Parse(res);
                var root = doc.RootElement;

                var messageId = root
                    .GetProperty("messages")[0]
                    .GetProperty("id")
                    .GetString();
                Console.WriteLine($"Message ID: {messageId}");

                if(messageId != null){
                    var save = await _repo.SaveOtpAsync(request.UserId.ToString(),request.To,otp.ToString(), messageId.ToString());
                    if(save)
                    {
                        return Ok(new { success = true, to = request.To, message = otpMessage, response = messageId });
                    }
                    else
                    {
                        return BadRequest("simpan OTP Error");
                    }
                }
                else
                {
                    return BadRequest("Tidak ada respon dari meta api");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal kirim pesan WhatsApp");
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, error = "Gagal kirim pesan WhatsApp" });
            }
        }

        /// <summary>
        /// Fungsi helper untuk kirim pesan WA via Cloud API
        /// </summary>
        private async Task<string> SendWhatsappMessageAsync(string to, string message)
        {
            var phoneNumberId = _configuration["Whatsapp:PhoneNumberId"];
            var token = _configuration["Whatsapp:AccessToken"];

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                messaging_product = "whatsapp",
                to = to,
                type = "text",
                text = new { body = message }
            };

            var response = await client.PostAsJsonAsync(
                $"https://graph.facebook.com/v23.0/{phoneNumberId}/messages",
                payload
            );

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Balasan WA: {Result}", result);

            return result;
        }

        private async Task<string> SendWhatsappMessageOTPAsync(string to, string otp, string bodyMessage)
        {
            var phoneNumberId = _configuration["Whatsapp:PhoneNumberId"];
            var token = _configuration["Whatsapp:AccessToken"];

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                messaging_product = "whatsapp",
                to = to,
                type = "interactive",
                interactive = new
                {
                    type = "button",
                    body = new { text = bodyMessage },
                    action = new
                    {
                        buttons = new[]
                        {
                            new {
                                type = "reply",
                                reply = new {
                                    id = "copy_otp_" + otp,   // id unik, bisa OTP
                                    title = "📋 Salin Kode"
                                }
                            }
                        }
                    }
                }
            };

            var response = await client.PostAsJsonAsync(
                $"https://graph.facebook.com/v23.0/{phoneNumberId}/messages",
                payload
            );

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Balasan WA: {Result}", result);

            return result;
        }


    }

    
}
