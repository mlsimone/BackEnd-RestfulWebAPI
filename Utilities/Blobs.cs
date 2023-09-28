using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using BackSide.Models;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Threading.Channels;

namespace BackSide.Utilities
{
    public class BlobStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _blobAccount;
        public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _blobServiceClient = blobServiceClient;

            _blobAccount = _configuration["AzureBlobStorageAccount"]; // Configuration.GetValue<String>("AzureBlobStorageAccount");

            // create the root container
            CreateRootContainer(_blobServiceClient);
        }

        // MLS ToDo: 9/26/23
        public void SaveImageToAzureBlobStorage(IFormFile image, String imageDirectory, string fileName)
        {
            // all container names must be lower case
            string containerName = imageDirectory.ToLower();
            string blobName = fileName;
            Uri containerUri = new Uri($"{_blobAccount}/{containerName}");

            BlobContainerClient containerClient;
            BlobContainerClient containerClient2;
            BlobClient blobClient;
            TokenCredential credential = new DefaultAzureCredential();
            Boolean isExist;

            // MLS I am confused about whether or not the container gets created when methods to 
            // get clientContainer are called?
            // I believe the containerClient can exist independent of the container. 

            // Create the containerClient2
            // MLS does this create the container if it doesn't exist?
            
            containerClient2 = new(containerUri, credential, default);
            isExist = ContainerNameExists(containerName);

            // This function (CreateIfNotExists) needs to have an instance of the containerClient to create a container so I believe 
            // it's possible for the client to exist independent of the container
            // The CreateIfNotExists(PublicAccessType, IDictionary<String, String>, BlobContainerEncryptionScopeOptions, CancellationToken)
            // operation creates a new container under the specified account.
            // If the container with the same name already exists, it is not changed.
            containerClient2.CreateIfNotExists(PublicAccessType.None, default, default, default);
            isExist = ContainerNameExists(containerName);

            containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            if (!ContainerNameExists(containerName))
                containerClient = _blobServiceClient.CreateBlobContainer(containerName);
            else
                containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Gets a reference to a BlobClient object by calling the GetBlobClient method on the container from the Create a container section.
            blobClient = containerClient.GetBlobClient(blobName);

            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            // Uploads the local text file to the blob by calling the UploadAsync method.
            // This method creates the blob if it doesn't already exist, and overwrites it if it does.

            // MLS ToDo:
            // How do you convert an IFormFile to a stream?
            blobClient.Upload(image.OpenReadStream(), true);

        }

        // MLS ToDo: 9/26/23
        public async Task<string> GetImageFromAzureBlobStorageAsBase64Async(string imageDirectory, string fileName)
        {
            string containerName = imageDirectory.ToLower();
            string blobName = fileName;
            
            Uri containerUri = new Uri($"{_blobAccount}/{containerName}");
            Uri blobUri = new Uri($"{_blobAccount}/{containerName}/{fileName}");

            TokenCredential credential = new DefaultAzureCredential();

            BlobClient blobClient = new BlobClient(blobUri, credential, default);

            string base64String = String.Empty;



            // These streams don't support writing and will throw an exception on this call ... await stream.WriteAsync(imageBytes);
            // using (Stream stream = blobClient.OpenRead())
            // using (Stream stream = await blobClient.OpenReadAsync())
            // This throws an exception about not being able to convert data from the bloblClient.OpenReadAsync call into a MemoryStream
            // using (MemoryStream m = (MemoryStream) await blobClient.OpenReadAsync())
            using (MemoryStream m = new MemoryStream())
            {
                // since the above streams were read-only,
                // copied them to a memory stream which is writable (below)
                blobClient.OpenRead().CopyTo(m); 

                byte[] imageBytes = m.ToArray();
                // append this before the base64 image representation: data: image / jpeg; base64
                base64String = "data: image / jpeg; base64," + Convert.ToBase64String(imageBytes);

                return base64String;
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
