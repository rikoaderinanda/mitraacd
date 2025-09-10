using System;
using System.ComponentModel.DataAnnotations;

namespace mitraacd.Models
{
    public class GoogleTokenRequest
    {
        public string IdToken { get; set; }
    }

    public class GoogleTokenPayload
    {
        public string Sub { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public string Aud { get; set; }
    }

    public class GoogleNewUser
    {
        public string UserIdGoogle { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
    }

    public class StatusRequest
    {
        public string userId { get; set; }
        public string Pesan { get; set; }
    }

    public class GeocodingResponse
    {
        public List<Result> Results { get; set; }
        public string Status { get; set; }
    }

    public class Result
    {
        public List<AddressComponent> Address_Components { get; set; }
        public string Formatted_Address { get; set; }
    }

    public class AddressComponent
    {
        public string Long_Name { get; set; }
        public string Short_Name { get; set; }
        public List<string> Types { get; set; }
    }
}