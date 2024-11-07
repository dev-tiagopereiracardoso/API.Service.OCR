namespace API.Service.OCR.Models.Output
{
    public class OcrOutput
    {
        public bool Success {  get; set; }

        public bool Error { get; set; }

        public string Message { get; set; }

        public string Text { get; set; }
    }
}