using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace CRMFlowMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connection string to Dataverse (CRM)
            string connectionString = "AuthType=OAuth; ..."; // Fill this with your actual connection string details

            CrmServiceClient serviceClient = new CrmServiceClient(connectionString);

            if (serviceClient.IsReady)
            {
                Console.WriteLine("Connected to Dataverse successfully!");

                // Query to retrieve all past process runs (including both modern flows and classic workflows)
                QueryExpression query = new QueryExpression("workflowlog")
                {
                    ColumnSet = new ColumnSet(true),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("status", ConditionOperator.Equal, 3) // 3: Completed, or use another status as required
                        }
                    },
                    Orders =
                    {
                        new OrderExpression("createdon", OrderType.Descending) // Order by created date descending
                    }
                };

                EntityCollection workflowLogs = serviceClient.RetrieveMultiple(query);

                foreach (Entity workflowLog in workflowLogs.Entities)
                {
                    Console.WriteLine("Workflow Name: " + workflowLog.GetAttributeValue<string>("name"));
                    Console.WriteLine("Status: " + workflowLog.GetAttributeValue<OptionSetValue>("statuscode").Value);
                    Console.WriteLine("Created On: " + workflowLog.GetAttributeValue<DateTime>("createdon"));
                    Console.WriteLine("----------------------------------------");
                }
            }
            else
            {
                Console.WriteLine("Failed to connect to Dataverse. Please check your connection details.");
            }
        }
    }
}