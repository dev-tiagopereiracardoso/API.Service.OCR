using API.Service.OCR.Domain.Implementation.Interfaces;
using API.Service.OCR.Models.Input;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Service.OCR.Service.Controllers
{
    [Route("v1/process/ocr")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "ocr")]
    public class ProcessOcrController : ControllerBase
    {
        private readonly IProcessOcrService _processOcrService;

        public ProcessOcrController(
                IProcessOcrService processOcrService
            )
        {
            _processOcrService = processOcrService;
        }

        [HttpPost("tesseract")]
        [SwaggerOperation(Summary = "")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status417ExpectationFailed)]
        public async Task<IActionResult> ProcessFileTesseract(FileUploadInput uploadfile)
        {
            var Data = _processOcrService.PostFileAsync(uploadfile).Result;

            if (Data.Success)
                return Ok(new
                {
                    Text = Data.Text
                });

            if(Data.Error)
                return BadRequest(new
                {
                    Message = Data.Message
                });

            return BadRequest();
        }

        [HttpPost("chatgpt")]
        [SwaggerOperation(Summary = "")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status417ExpectationFailed)]
        public async Task<IActionResult> ProcessFileChatGpt(FileUploadChatGptInput uploadfile)
        {
            var Data = _processOcrService.PostFileChatGptAsync(uploadfile).Result;

            if (Data.Success)
                return Ok(new
                {
                    Text = Data.Text
                });

            if (Data.Error)
                return BadRequest(new
                {
                    Message = Data.Message
                });

            return BadRequest();
        }
    }
}