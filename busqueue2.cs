using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System.Text.Json;

public static class ServiceBusQueueTriggerFunction
{
    [FunctionName("ServiceBusQueueTriggerFunction")]
    public static void Run(
        [ServiceBusTrigger("second-function-queue", Connection = "ServiceBusConnectionString")] string myQueueItem,
        ILogger log)
    {
        var message = JsonSerializer.Deserialize<dynamic>(myQueueItem);
        var directoryName = message?.DirectoryName;
        log.LogInformation($"Processing directory: {directoryName}");

        // Add your processing logic here
    }
}