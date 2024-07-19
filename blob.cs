using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System.Threading.Tasks;

public static class BlobTriggerFunction
{
    [FunctionName("BlobTriggerFunction")]
    public static async Task Run(
        [BlobTrigger("your-container-name/{name}", Connection = "AzureWebJobsStorage")] BlobClient blobClient,
        string name,
        ILogger log)
    {
        if (blobClient.GetParentBlobContainerClient().GetBlobDirectoryClient(name).Exists())
        {
            log.LogInformation($"New directory detected: {name}");

            // Trigger the second function to run after 1 hour
            var timeToRun = DateTimeOffset.UtcNow.AddHours(1);
            var message = new { DirectoryName = name };
            await ServiceBusQueueSender.SendMessageAsync("second-function-queue", message, timeToRun, log);
        }
    }
}