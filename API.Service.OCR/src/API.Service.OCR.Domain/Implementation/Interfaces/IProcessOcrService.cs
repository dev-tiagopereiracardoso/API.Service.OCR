using API.Service.OCR.Models.Input;
using API.Service.OCR.Models.Output;

namespace API.Service.OCR.Domain.Implementation.Interfaces
{
    public interface IProcessOcrService
    {
        Task<OcrOutput> PostFileAsync(FileUploadInput uploadfile);
    }
}