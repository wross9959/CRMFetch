using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

public static class ServiceBusQueueSender
{
    private static string ServiceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
    private static string QueueName = "second-function-queue";

    public static async Task SendMessageAsync(string queueName, object message, DateTimeOffset scheduleEnqueueTimeUtc, ILogger log)
    {
        var client = new ServiceBusClient(ServiceBusConnectionString);
        var sender = client.CreateSender(queueName);
        var jsonMessage = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
        {
            ScheduledEnqueueTime = scheduleEnqueueTimeUtc
        };

        try
        {
            await sender.SendMessageAsync(serviceBusMessage);
            log.LogInformation($"Scheduled message to queue '{queueName}' at {scheduleEnqueueTimeUtc}");
        }
        catch (Exception ex)
        {
            log.LogError($"Error sending message: {ex.Message}");
        }
        finally
        {
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }
}