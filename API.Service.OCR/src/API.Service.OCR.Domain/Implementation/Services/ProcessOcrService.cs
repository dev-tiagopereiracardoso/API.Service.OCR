using API.Service.OCR.Domain.Implementation.Interfaces;
using API.Service.OCR.Models.Dto;
using API.Service.OCR.Models.Input;
using API.Service.OCR.Models.Output;
using Aspose.Pdf;
using Aspose.Pdf.Devices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Drawing;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;
using Color = System.Drawing.Color;

namespace API.Service.OCR.Domain.Implementation.Services
{
    public class ProcessOcrService : IProcessOcrService
    {
        private readonly IConfiguration _configuration;

        public ProcessOcrService(
                IConfiguration configuration
            )
        {
            _configuration = configuration;
        }

        public async Task<OcrOutput> PostFileAsync(FileUploadInput uploadfile)
        {
            var ObjReturn = new OcrOutput();

            try
            {
                var uidFolder = Guid.NewGuid().ToString();
                var pathFileCurrent = _configuration["PathFileTemp"] + uidFolder;
                var file = await NewFile(uploadfile.file);

                if (uploadfile == null || uploadfile.file.Length == 0)
                {
                    ObjReturn.Error = true;
                    ObjReturn.Message = "No files were selected for upload.";

                    return ObjReturn;
                }
                else if (file.Extension.Equals(".pdf"))
                {
                    CreateFolder(pathFileCurrent);

                    Document pdfDocument = new Document((MemoryStream)file.Content);

                    foreach (var page in pdfDocument.Pages)
                    {
                        Resolution resolution = new Resolution(300);
                        PngDevice PngDevice = new PngDevice(500, 700, resolution);
                        PngDevice.Process(pdfDocument.Pages[page.Number], pathFileCurrent + "\\image" + page.Number + "_out" + ".Png");
                    }
                }
                else if (file.Extension.Equals(".jpg") || file.Extension.Equals(".png"))
                {
                    CreateFolder(pathFileCurrent);

                    using (var fileStream = File.Create(pathFileCurrent + "\\image1_out" + file.Extension))
                    {
                        file.Content.Seek(0, SeekOrigin.Begin);
                        file.Content.CopyTo(fileStream);
                    }
                }
                else
                {
                    ObjReturn.Error = true;
                    ObjReturn.Message = "The upload extension is not valid. Allowed extension .pdf, .jpg, .png";

                    return ObjReturn;
                }

                DirectoryInfo folderWithFiles = new DirectoryInfo(pathFileCurrent);

                for (int i = 1; i < folderWithFiles.GetFiles().Length + 1; i++)
                {
                    using (var engine = new TesseractEngine(_configuration["PathTessData"], "eng", EngineMode.Default))
                    {
                        var caminhoImagem = pathFileCurrent + "\\image" + i + "_out" + ((file.Extension == ".pdf") ? ".png" : file.Extension);

                        engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZÇabcdefghijklmnopqrstuvwxyzç0123456789");
                        engine.SetVariable("tessedit_pageseg_mode", "1");

                        var imagemOriginal = new Bitmap(caminhoImagem);

                        var imagemProcessada = ConverterParaEscalaCinza(imagemOriginal);
                        imagemProcessada = BinarizarImagem(imagemProcessada);

                        var pixImagem = ConvertBitmapToPix(imagemProcessada);
                        var page = engine.Process(pixImagem);
                        var text = page.GetText();

                        ObjReturn.Text += text;

                        imagemOriginal.Dispose();
                        imagemProcessada.Dispose();

                        pixImagem.Dispose();
                    }
                }

                Directory.Delete(pathFileCurrent, true);

                ObjReturn.Success = true;
            }
            catch (Exception Ex)
            {
                _ = Ex.Message;
                _ = Ex.StackTrace;

                ObjReturn.Error = true;
                ObjReturn.Message = Ex.Message + "|" + Ex.StackTrace;
            }

            return ObjReturn;
        }

        public async Task<OcrOutput> PostFileChatGptAsync(FileUploadChatGptInput uploadfile)
        {
            var ObjReturn = new OcrOutput();

            try
            {
                var uidFolder = Guid.NewGuid().ToString();
                var pathFileCurrent = _configuration["PathFileTemp"] + uidFolder;
                var file = await NewFile(uploadfile.file);

                if (uploadfile == null || uploadfile.file.Length == 0)
                {
                    ObjReturn.Error = true;
                    ObjReturn.Message = "No files were selected for upload.";

                    return ObjReturn;
                }
                else if (file.Extension.Equals(".pdf"))
                {
                    CreateFolder(pathFileCurrent);

                    Document pdfDocument = new Document((MemoryStream)file.Content);

                    foreach (var page in pdfDocument.Pages)
                    {
                        Resolution resolution = new Resolution(300);
                        PngDevice PngDevice = new PngDevice(500, 700, resolution);
                        PngDevice.Process(pdfDocument.Pages[page.Number], pathFileCurrent + "\\image" + page.Number + "_out" + ".Png");
                    }
                }
                else if (file.Extension.Equals(".jpg") || file.Extension.Equals(".png"))
                {
                    CreateFolder(pathFileCurrent);

                    using (var fileStream = File.Create(pathFileCurrent + "\\image1_out" + file.Extension))
                    {
                        file.Content.Seek(0, SeekOrigin.Begin);
                        file.Content.CopyTo(fileStream);
                    }
                }
                else
                {
                    ObjReturn.Error = true;
                    ObjReturn.Message = "The upload extension is not valid. Allowed extension .pdf, .jpg, .png";

                    return ObjReturn;
                }

                DirectoryInfo folderWithFiles = new DirectoryInfo(pathFileCurrent);

                var urlFilesTemp = _configuration["UrlFilesTemp"];
                var urlApiChatGpt = _configuration["Api:ChatGpt"];

                for (int i = 1; i < folderWithFiles.GetFiles().Length + 1; i++)
                {
                    var json = new
                    {
                        UrlImage = urlFilesTemp + "/image" + i + "_out" + ((file.Extension == ".pdf") ? ".png" : file.Extension),
                        Question = "Extract this receipt information into JSON array with following fields. The numeric values should not have a thousand separator. Just give me the plain JSON string without any markdown tag. Return me the properties in English without underscore"
                    };

                    var objString = JsonConvert.SerializeObject(json);
                    var request = new HttpRequestMessage(HttpMethod.Post, urlApiChatGpt);
                    request.Content = new StringContent(objString, Encoding.UTF8);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await new HttpClient().SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var result = await response.Content.ReadAsStringAsync();

                    ObjReturn.Text += result;
                }

                Directory.Delete(pathFileCurrent, true);

                ObjReturn.Success = true;
            }
            catch (Exception Ex)
            {
                _ = Ex.Message;
                _ = Ex.StackTrace;

                ObjReturn.Error = true;
                ObjReturn.Message = Ex.Message + "|" + Ex.StackTrace;
            }

            return ObjReturn;
        }

        private void CreateFolder(string pathFileCurrent)
        {
            bool exists = Directory.Exists(pathFileCurrent);

            if (!exists)
                Directory.CreateDirectory(pathFileCurrent);
        }

        private Pix ConvertBitmapToPix(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                return Pix.LoadFromMemory(ms.ToArray());
            }
        }

        private async Task<FileDto> NewFile(IFormFile formFile)
        {
            var file = new FileDto()
            {
                ContentLength = formFile.Length,
                ContentType = formFile.ContentType,
                Name = formFile.FileName
            };
            await formFile.CopyToAsync(file.Content, new CancellationToken());

            return file;
        }

        private Bitmap ConverterParaEscalaCinza(Bitmap imagem)
        {
            for (int i = 0; i < imagem.Width; i++)
            {
                for (int j = 0; j < imagem.Height; j++)
                {
                    Color pixel = imagem.GetPixel(i, j);
                    int media = (int)((pixel.R + pixel.G + pixel.B) / 3);
                    imagem.SetPixel(i, j, Color.FromArgb(media, media, media));
                }
            }
            return imagem;
        }

        private static Bitmap BinarizarImagem(Bitmap imagem)
        {
            Bitmap binarizada = new Bitmap(imagem);
            for (int i = 0; i < imagem.Width; i++)
            {
                for (int j = 0; j < imagem.Height; j++)
                {
                    Color pixel = imagem.GetPixel(i, j);
                    int media = (int)((pixel.R + pixel.G + pixel.B) / 3);
                    if (media > 128)
                        binarizada.SetPixel(i, j, Color.White);
                    else
                        binarizada.SetPixel(i, j, Color.Black);
                }
            }
            return binarizada;
        }

    }
}