using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace API.Service.OCR.Models.Input
{
    public class FileUploadChatGptInput
    {
        public string Text { get; set; } = String.Empty;

        [JsonPropertyName("@file")]
        public IFormFile file { get; set; }
    }
}