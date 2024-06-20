using System;
using System.Collections.Generic;
using System.Configuration; // Ensure this is included
using System.IO;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

//nu packs needed
//System.Configuration.ConfigurationManager
//Microsoft.CrmSdk.CoreAssemblies
//Microsoft.CrmSdk.XrmTooling.CoreAssembly

class Program
{
    static void Main(string[] args)
    {
        bool useMockData = true; // Set this to false to use real CRM data

        try
        {
            if (useMockData)
            {
                Console.WriteLine("Using mock data...");
                // Use mock data for testing
                EntityCollection mockData = GetMockData();

                foreach (var entity in mockData.Entities)
                {
                    string annotationId = entity["annotationid"].ToString();
                    string filename = entity.Contains("filename") ? entity["filename"].ToString() : $"{annotationId}.dat";
                    string documentbody = entity["documentbody"].ToString();

                    WriteFile(documentbody, filename);
                }
            }
            else
            {
                Console.WriteLine("Connecting to CRM...");
                // Use real CRM data
                string connectionString = ConfigurationManager.ConnectionStrings["CrmConnectionString"].ConnectionString;

                CrmServiceClient serviceClient = new CrmServiceClient(connectionString);

                if (serviceClient.IsReady)
                {
                    Console.WriteLine("Connected to CRM.");
                    string fetchXml = @"
                    <fetch mapping='logical'>
                       <entity name='annotation'>
                          <attribute name='annotationid' />
                          <attribute name='filename' />
                          <attribute name='documentbody' />
                          <attribute name='filesize' />
                          <attribute name='mimetype' />
                          <attribute name='objectid' />
                          <attribute name='objecttypecode' />
                          <attribute name='objecttypecodename' />
                          <filter>
                             <condition attribute='isdocument' operator='eq' value='true' />
                          </filter>
                       </entity>
                    </fetch>";

                    EntityCollection results = serviceClient.RetrieveMultiple(new FetchExpression(fetchXml));

                    foreach (var entity in results.Entities)
                    {
                        string annotationId = entity["annotationid"].ToString();
                        string filename = entity.Contains("filename") ? entity["filename"].ToString() : $"{annotationId}.dat";
                        string documentbody = entity["documentbody"].ToString();

                        WriteFile(documentbody, filename);
                    }
                }
                else
                {
                    Console.WriteLine("Failed to connect to CRM.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        Console.WriteLine("Processing complete. Press any key to exit.");
        Console.ReadKey();
    }

    static EntityCollection GetMockData()
    {
        var entity1 = new Entity("annotation")
        {
            ["annotationid"] = Guid.NewGuid(),
            ["filename"] = "testfile1.txt",
            ["documentbody"] = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("This is a test file 1"))
        };

        var entity2 = new Entity("annotation")
        {
            ["annotationid"] = Guid.NewGuid(),
            ["filename"] = "testfile2.png",
            ["documentbody"] = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("This is a test file 2"))
        };

        var entityCollection = new EntityCollection(new List<Entity> { entity1, entity2 });
        return entityCollection;
    }

    static void WriteFile(string fileContents, string fileName)
    {
        try
        {
            byte[] data = Convert.FromBase64String(fileContents);
            string outputDirectory = ConfigurationManager.AppSettings["OutputDirectory"];
            string filePath = Path.Combine(outputDirectory, fileName);
            File.WriteAllBytes(filePath, data);
            Console.WriteLine($"File {fileName} written to {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while writing the file {fileName}: {ex.Message}");
        }
    }
}
