using API.Service.OCR.Models.Enums;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace API.Service.OCR.Models.Input
{
    public class FileUploadInput
    {
        [JsonPropertyName("@file")]
        public IFormFile file { get; set; }
    }
}