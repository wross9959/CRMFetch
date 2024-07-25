Migrating your on-premises SharePoint data to SharePoint Online using .NET and C# involves several steps. Given your constraints (no client ID, MFA, and blocked older authentication), you can use modern authentication methods like OAuth 2.0 with your existing MFA setup. Here’s a general approach to achieve this:

### Step-by-Step Guide:

1. **Set Up Azure AD App Registration:**
   - Register an application in Azure AD to get the necessary permissions to access SharePoint Online.
   - Configure the application with the necessary API permissions (e.g., SharePoint Online permissions).

2. **Get Access Token with MFA:**
   - Use device code flow or interactive login flow to obtain an access token. These flows support MFA and don't require a client ID and secret in your code.

3. **Connect to On-Premises SharePoint:**
   - Use CSOM (Client-Side Object Model) or REST API to connect to your on-premises SharePoint sites.
   - Use your on-premises credentials to authenticate (ensure your credentials have sufficient access).

4. **Migrate Data:**
   - Fetch data from the on-premises SharePoint.
   - Use the SharePoint Online CSOM or REST API to upload the data to SharePoint Online.

### Sample Code to Connect to SharePoint Online with MFA

Here's an example using MSAL.NET to get an access token interactively:

```csharp
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
```

### Sample Code to Connect to On-Premises SharePoint

Here's an example using CSOM to connect to SharePoint on-premises:

```csharp
using Microsoft.SharePoint.Client;
using System;
using System.Net;

class Program
{
    static void Main(string[] args)
    {
        string siteUrl = "http://your-on-prem-sharepoint-site";
        string username = "your-username";
        string password = "your-password";

        using (ClientContext context = new ClientContext(siteUrl))
        {
            context.Credentials = new NetworkCredential(username, password);

            Web web = context.Web;
            context.Load(web);
            context.ExecuteQuery();
            Console.WriteLine("Title: " + web.Title);

            // Fetch and migrate data
        }
    }
}
```

### Tips:

- **Test in a Staging Environment:** Before running the migration in production, test it in a staging environment to ensure that the data migration process works correctly.
- **Handle Large Lists:** If you're migrating large lists, consider using batching to improve performance and avoid throttling.
- **Error Handling:** Implement robust error handling to capture and log any issues during the migration process.
- **Data Mapping:** Ensure that the data structures in your on-premises SharePoint are compatible with SharePoint Online.

This approach leverages modern authentication for SharePoint Online while using traditional authentication methods for on-premises SharePoint, ensuring compatibility with your MFA setup and blocked older authentication methods.
