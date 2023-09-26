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
        public readonly string BaseImageDirectory;
        private readonly IConfiguration Configuration;
#if (Use_Azure_Blob_Storage == true)
        private readonly BlobStorageService _blobStorageService;
        public FileStorageService(IConfiguration configuration,
                                    BlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
#else
        public FileStorageService(IConfiguration configuration) {
#endif
            Configuration = configuration;
            BaseImageDirectory = Configuration["ImageDirectory"]; // var defaultLogLevel = Configuration["Logging:LogLevel:Default"];
            // MLS 7/26/23 below gave null result -- didn't work
            // System.Configuration.ConfigurationManager.AppSettings["ImageDirectory"];

        }
        public string GetImageAsB64String(string imageDirectory, string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return "";

            // return the image in "imagename" as a base64 encoded string
            string base64String;
            string directory = Path.Combine(BaseImageDirectory, imageDirectory);
            string filePath = Path.Combine(directory, imageName);
#if (Use_Azure_Blob_Storage != true)
            base64String = GetImageFromHardDriveAsBase64(filePath);
#else
            base64String = _blobStorageService.GetImageFromAzureBlobStorageAsBase64(filePath);
#endif
            return base64String;
        }

        public void SaveImage(IFormFile image, string imageDirectory)
        {
            string directory = Path.Combine(BaseImageDirectory, imageDirectory);
            string fileName = image.FileName;
            var filePath = Path.Combine(directory, fileName);

            if (image.Length > 0)
            {
                // MLS 6/20/23 Removed weird fileName characters in Front End
                // there's a weird IMG_155.JPG:046398: tacked on to the end of the filename
                // this logic strips off the stuff after the :
                // int len = formFile.FileName.IndexOf(":");
                // string fileName = formFile.FileName.Substring(0,len);
#if (Use_Azure_Blob_Storage != true)
                SaveImageToHardDrive(image, directory, filePath);
#else
                _blobStorageService.SaveImageToAzureBlobStorage(image, directory, filePath);
#endif
            }
        }

        public void SaveImageToHardDrive(IFormFile image, string imageDirectory, string filePath)
        {
            if (!Directory.Exists(imageDirectory)) Directory.CreateDirectory(imageDirectory);

            if (!System.IO.File.Exists(filePath))
            {
                using (var stream = System.IO.File.Create(filePath))
                {
                    image.CopyTo(stream);

                }
            }
        }

        public string GetImageFromHardDriveAsBase64(string filePath)
        {
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
