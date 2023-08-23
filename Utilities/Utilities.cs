using System.IO;
using BackSide.Models;
using System.Text;
using System.Drawing;
using Microsoft.AspNetCore.Http;

namespace BackSide.Utilities
{
    public static class Files
    {
        public static string GetImageFromHDandReturnB64String(string directory, string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return "";

            // return the image in "imagename" as a base64 encoded string
            string base64String;
            var filePath = Path.Combine(directory, imageName);
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(filePath))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // append this before the base64 image representation: data: image / jpeg; base64
                    base64String = "data: image / jpeg; base64," + Convert.ToBase64String(imageBytes);
                }
            }
            return base64String;
        }

        public static void SaveImageToHardDrive(IFormFile image, string imageDirectory)
        {

            string fileName = image.FileName;
            var filePath = Path.Combine(imageDirectory, fileName);

            if (image.Length > 0)
            {
                // MLS 6/20/23 Removed weird fileName characters in Front End
                // there's a weird IMG_155.JPG:046398: tacked on to the end of the filename
                // this logic strips off the stuff after the :
                // int len = formFile.FileName.IndexOf(":");
                // string fileName = formFile.FileName.Substring(0,len);

                if (!Directory.Exists(imageDirectory)) Directory.CreateDirectory(imageDirectory);

                // MLS 6/20/23 ToDo: save IMAGENames to Images table (Will need to create IMAGES table)
                if (!System.IO.File.Exists(filePath))
                {
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        image.CopyTo(stream);

                    }
                }
            }
        }
    }

}
