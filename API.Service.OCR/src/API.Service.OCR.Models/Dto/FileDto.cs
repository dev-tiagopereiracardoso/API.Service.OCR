namespace API.Service.OCR.Models.Dto
{
    public class FileDto
    {
        public FileDto()
        {
            this.Content = (Stream)new MemoryStream();
        }

        public string Name { get; set; }

        public Stream Content { get; set; }

        public string ContentType { get; set; }

        public long ContentLength { get; set; }

        public string Extension
        {
            get
            {
                return Path.GetExtension(this.Name);
            }
        }
    }
}
