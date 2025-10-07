using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using mitraacd.Models;
using mitraacd.Services;
using Newtonsoft.Json.Linq;

namespace mitraacd.api
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly HttpClient _http;
        private readonly string _googleApiKey = "AIzaSyBS9s8GewgkGEaj3ANIwMaTOmZCYbd-aR0";
        private readonly ILocationRepo _repo;

        public LocationController(HttpClient http,ILocationRepo repo)
        {
            _http = http;
            _repo = repo;
        }

        [HttpGet("reverse-geocode")]
        public async Task<IActionResult> ReverseGeocode(double lat, double lng)
        {
            // lat = -6.5973453;
            // lng = 106.7659312;
            string apiKey = _googleApiKey;
            string url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}&key={apiKey}";

            var response = await _http.GetFromJsonAsync<GeocodingResponse>(url);

            if (response?.Status == "OK")
            {
                string city = "";
                string province = "";
                string kecamatan = "";
                string kelurahan = "";
                string fullAddress = response.Results[0].Formatted_Address;

                foreach (var component in response.Results[0].Address_Components)
                {
                    if (component.Types.Contains("administrative_area_level_5") || 
                        component.Types.Contains("administrative_area_level_4") || 
                        component.Types.Contains("sublocality_level_1"))
                    {
                        kelurahan = component.Long_Name;
                    }
                    
                    if (component.Types.Contains("administrative_area_level_3"))
                        kecamatan = component.Long_Name;
                    if (component.Types.Contains("administrative_area_level_2"))
                        city = component.Long_Name;
                    if (component.Types.Contains("administrative_area_level_1"))
                        province = component.Long_Name;
                }
                return Ok(new { 
                    City = Singkatkan(city), 
                    Province = province,
                    Kecamatan = Singkatkan(kecamatan),
                    Kelurahan = kelurahan,
                    FullAddress = fullAddress});
            }

            return BadRequest(response);
        }
        
        [HttpGet("NearbyKecamatan")]
        public async Task<IActionResult> NearbyKecamatan(double lat, double lng, int radiusKm = 1)
        {
            var res = await _repo.NearbyKecamatan(lat,lng,radiusKm);
            return Ok(res);
        }

        private static string Singkatkan(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            if (input.StartsWith("Kabupaten"))
                return input.Replace("Kabupaten", "Kab.");
            if (input.StartsWith("Kota"))
                return input.Replace("Kota", "Kota."); // atau "Kota" saja kalau nggak mau ada titik
            if (input.StartsWith("Kecamatan"))
                return input.Replace("Kecamatan", "Kec.");

            return input;
        }

        [HttpPost("simpanCoverageArea")]
        public async Task<ActionResult<dynamic>> simpanCoverageArea([FromBody] ReqsimpanCoverageArea req)
        {
            var res = await _repo.simpanCoverageArea(req);

            if (res)
            {
                return Ok(new {
                    success = true,
                    message = "Data berhasil disimpan"
                });
            }

            return BadRequest(new {
                success = false,
                message = "Data gagal disimpan",
                data = req
            });
        }

        [HttpGet("CheckRadiusExisting")]
        public async Task<IActionResult> CheckRadiusExisting(string Id)
        {
            var res = await _repo.CheckRadiusExisting(Id);
            return Ok(res);
        }
    }
}