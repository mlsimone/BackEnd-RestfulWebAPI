using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BackSide.Models;

namespace BackSide.Utilities
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        BlobStorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;

            // create the root container
            CreateRootContainer(_blobServiceClient);
        }

        // MLS ToDo: 9/26/23
        public void SaveImageToAzureBlobStorageAsync(IFormFile image, String imageDirectory, string filePath)
        {
            BlobContainerClient containerClient;
            BlobContainerItem containerItem;

            // all container names must be lower case
            string containerName = imageDirectory.ToLower();

            if (!ContainerNameExists(containerName)) containerClient = _blobServiceClient.CreateBlobContainer(containerName);

            if (!BlobNameExists(image.FileName, containerClient))
            {
                // Get a reference to a blob
                BlobClient blobClient = containerClient.GetBlobClient(filePath);

                Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

                // Upload data from the local file
                blobClient.Upload(filePath, true);
            }

        }

        // MLS ToDo: 9/26/23
        public string GetImageFromAzureBlobStorageAsBase64(string filePath)
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

        public Boolean BlobNameExists(string blobName, BlobContainerClient container)
        {
            try
            {
                foreach (BlobItem blobItem in container.GetBlobs())
                {
                    if (blobItem.Name.Equals(blobName)) return true;
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public Boolean ContainerNameExists(string containerName) //, out BlobContainerItem containerItem)
        {
            // containerItem = null;
            try
            {
                foreach (BlobContainerItem container in _blobServiceClient.GetBlobContainers())
                {
                    if (container.Name.Equals(containerName))
                    {
                        // containerItem = container;
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        // From Microsoft: see https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-container-create
        //-------------------------------------------------
        // Create root container
        //-------------------------------------------------
        private static void CreateRootContainer(BlobServiceClient blobServiceClient)
        {
            try
            {
                // Create the root container or handle the exception if it already exists
                BlobContainerClient container = blobServiceClient.CreateBlobContainer("$root");

                if (container.Exists())
                {
                    Console.WriteLine("Created root container.");
                }
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine("HTTP error code {0}: {1}",
                                    e.Status, e.ErrorCode);
                Console.WriteLine(e.Message);
            }
        }


    }
}
