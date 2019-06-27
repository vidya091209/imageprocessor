using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessor
{
    public static class Function1
    {
        static string storageAccountConnectionString = System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        static string thumbContainerName = System.Environment.GetEnvironmentVariable("myContainerName");

        [FunctionName("Function1")]
        public static async Task Run([BlobTrigger("rawimages/{name}", Connection = "")]Stream inputBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {inputBlob.Length} Bytes");
          

            // Get the blobname from the event's JObject. updated PR
            string blobName = name;

            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(thumbContainerName);
            await container.CreateIfNotExistsAsync();
            // Create reference to a blob named "blobName".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            var format = Image.DetectFormat(inputBlob);

            using (Image<Rgba32> img = Image.Load(inputBlob))
            {
                img.Mutate(ctx => ctx.Resize(150, 150));
                using (var stream = await blockBlob.OpenWriteAsync())
                {
                    img.Save(stream, format);
                }

            }
                     
        }
    }
}
