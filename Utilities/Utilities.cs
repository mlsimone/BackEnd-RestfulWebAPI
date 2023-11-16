#define Use_Azure_Blob_Storage
using System.IO;
using BackSide.Models;
using System.Text;
using System.Drawing;
using Microsoft.AspNetCore.Http;
using static System.Net.Mime.MediaTypeNames;
using System.Configuration;

namespace BackSide.Utilities
{
    public class FileStorageService
    {
        public readonly string _baseImageDirectory;
        private readonly IConfiguration _configuration;
#if Use_Azure_Blob_Storage
        private readonly BlobStorageService _blobStorageService;
        public FileStorageService(IConfiguration configuration,
                                    BlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
#else
        public FileStorageService(IConfiguration configuration) {
#endif
            _configuration = configuration;
            _baseImageDirectory = _configuration["ImageDirectory"]; // var defaultLogLevel = Configuration["Logging:LogLevel:Default"];
            // MLS 7/26/23 below gave null result -- didn't work
            // System.Configuration.ConfigurationManager.AppSettings["ImageDirectory"];

        }
        public async Task<string> GetImageAsB64String(string imageDirectory, string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return "";

            // return the image in "imagename" as a base64 encoded string
            string base64String;

#if Use_Azure_Blob_Storage
            base64String = await _blobStorageService.GetImageFromAzureBlobStorageAsBase64Async(imageDirectory, imageName);
#else
            base64String = GetImageFromHardDriveAsBase64(imageDirectory, imageName);
#endif
            return base64String;
        }

        public void SaveImage(IFormFile image, string imageDirectory)
        {
            string fileName = image.FileName;
            
            if (image.Length > 0)
            {
                // MLS 6/20/23 Removed weird fileName characters in Front End
                // there's a weird IMG_155.JPG:046398: tacked on to the end of the filename
                // this logic strips off the stuff after the :
                // int len = formFile.FileName.IndexOf(":");
                // string fileName = formFile.FileName.Substring(0,len);
#if Use_Azure_Blob_Storage
                _blobStorageService.SaveImageToAzureBlobStorage(image, imageDirectory, fileName);
#else
                SaveImageToHardDrive(image, imageDirectory, fileName);
#endif
            }
        }

        public void SaveImageToHardDrive(IFormFile image, string imageDirectory, string fileName)
        {
            string directory = Path.Combine(_baseImageDirectory, imageDirectory);
            var filePath = Path.Combine(directory, fileName);

            if (!Directory.Exists(imageDirectory)) Directory.CreateDirectory(directory);

            if (!System.IO.File.Exists(filePath))
            {
                using (var stream = System.IO.File.Create(filePath))
                {
                    image.CopyTo(stream);

                }
            }
        }

        public string GetImageFromHardDriveAsBase64(string imageDirectory, string imageName)
        {
            string directory = Path.Combine(_baseImageDirectory, imageDirectory);
            string filePath = Path.Combine(directory, imageName);

            string base64String = String.Empty;

            using (System.Drawing.Image image = System.Drawing.Image.FromFile(filePath))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // append this before the base64 image representation: data: image / jpeg; base64
                    base64String = "data: image / jpeg; base64," + Convert.ToBase64String(imageBytes);

                    return base64String;
                }
            }
        }

    }

}
