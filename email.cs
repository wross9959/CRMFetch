using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

public static class FolderMonitor
{
    [FunctionName("FolderMonitor")]
    public static async Task Run(
        [BlobTrigger("your-container-name/{name}", Connection = "AzureWebJobsStorage")] CloudBlockBlob myBlob,
        string name, ILogger log)
    {
        log.LogInformation($"New blob detected: {name}");

        // Check if it's a folder (In Azure Blob Storage, folders are simulated by blobs with names that include "/")
        if (myBlob.Name.EndsWith("/"))
        {
            // Check the content of the folder
            CloudBlobDirectory folder = myBlob.Container.GetDirectoryReference(myBlob.Name);
            BlobResultSegment resultSegment = await folder.ListBlobsSegmentedAsync(false, BlobListingDetails.None, null, null, null, null);

            if (resultSegment.Results.Count() == 0)
            {
                // Send email notification if folder is empty
                await SendEmail("Folder is empty", $"The folder {name} was created but it's empty.");
            }
            else
            {
                // Check if the folder has been there for an hour
                DateTime creationTime = myBlob.Properties.Created.Value.UtcDateTime;
                if ((DateTime.UtcNow - creationTime).TotalHours >= 1)
                {
                    await SendEmail("Folder contains data", $"The folder {name} has data and has been there for over an hour.");
                }
            }
        }
    }

    private static async Task SendEmail(string subject, string message)
    {
        var client = new SendGrid.SendGridClient(Environment.GetEnvironmentVariable("SendGridApiKey"));
        var msg = new SendGridMessage()
        {
            From = new EmailAddress("your-email@example.com", "CRM Monitor"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress("crm-team@example.com", "CRM Team"));

        var response = await client.SendEmailAsync(msg);
    }
}