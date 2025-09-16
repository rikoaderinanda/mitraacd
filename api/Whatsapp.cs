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
        /// Fungsi helper untuk kirim pesan WA via Cloud API
        /// </summary>
        private async Task SendWhatsappMessageAsync(string to, string message)
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
                $"https://graph.facebook.com/v22.0/{phoneNumberId}/messages",
                payload
            );

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Balasan WA: {Result}", result);
        }
    }
}
