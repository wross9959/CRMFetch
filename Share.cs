using Microsoft.Identity.Client;
using Microsoft.SharePoint.Client;
using System;
using System.Security;

class Program
{
    static async Task Main(string[] args)
    {
        string[] scopes = new string[] { "https://your-sharepoint-site/_api/.default" };
        var app = PublicClientApplicationBuilder.Create("your-client-id")
            .WithRedirectUri("http://localhost")
            .Build();

        var result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();

        using (ClientContext context = new ClientContext("https://your-sharepoint-site"))
        {
            context.AuthenticationMode = ClientAuthenticationMode.Default;
            context.ExecutingWebRequest += (sender, e) =>
            {
                e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + result.AccessToken;
            };

            Web web = context.Web;
            context.Load(web);
            context.ExecuteQuery();
            Console.WriteLine("Title: " + web.Title);
        }
    }
}
