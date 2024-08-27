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
                    ColumnSet = new ColumnSet("asyncoperationid", "name", "statuscode", "createdon"),
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
                    // Retrieve the associated async operation details using the asyncoperationid
                    EntityReference asyncOperationReference = workflowLog.GetAttributeValue<EntityReference>("asyncoperationid");

                    if (asyncOperationReference != null)
                    {
                        Entity asyncOperation = serviceClient.Retrieve(asyncOperationReference.LogicalName, asyncOperationReference.Id, new ColumnSet(true)); // Get all fields to debug

                        // Debugging: Print all fields of asyncOperation
                        foreach (var attribute in asyncOperation.Attributes)
                        {
                            Console.WriteLine($"Field: {attribute.Key}, Value: {attribute.Value}");
                        }

                        // Attempt to retrieve workflowid if it exists
                        if (asyncOperation.Attributes.Contains("workflowid"))
                        {
                            Guid workflowId = asyncOperation.GetAttributeValue<Guid>("workflowid");

                            if (workflowId != Guid.Empty)
                            {
                                Entity workflow = serviceClient.Retrieve("workflow", workflowId, new ColumnSet("name", "category", "type"));

                                Console.WriteLine("Workflow Name: " + workflow.GetAttributeValue<string>("name"));
                                Console.WriteLine("Workflow Category: " + workflow.GetAttributeValue<OptionSetValue>("category").Value);
                                Console.WriteLine("Workflow Type: " + workflow.GetAttributeValue<OptionSetValue>("type").Value);
                            }
                        }
                        else
                        {
                            Console.WriteLine("workflowid not found in asyncOperation entity.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("asyncoperationid is null.");
                    }

                    Console.WriteLine("Log Name: " + workflowLog.GetAttributeValue<string>("name"));
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
