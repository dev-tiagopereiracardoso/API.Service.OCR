using API.Service.OCR.Models.Enums;

namespace API.Service.OCR.Models.Dto
{
    public class FileDetailsDto
    {
        public string FileName { get; set; }

        public byte[] FileData { get; set; }

        public FileTypeEnum FileType { get; set; }
    }
}