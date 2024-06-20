using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Xml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using static System.Net.Mime.MediaTypeNames;

using Microsoft.Xrm.Sdk.Client;
//using System.ServiceModel.Description;
//nu packs needed
//System.Configuration.ConfigurationManager
//Microsoft.CrmSdk.CoreAssemblies
//Microsoft.CrmSdk.XrmTooling.CoreAssembly

class Program
{
    static void Main(string[] args)
    {
        // connect to sql
        string connectionString = ConfigurationManager.ConnectionStrings["CrmConnectionString"].ConnectionString;

        
        //EntityCollection aggregateResult = CrmService.RetrieveMultiple(new FetchExpression(strFetchXML));

    }




    static IEnumerable<Entity> ExecuteFetchXml(IOrganizationService service, string fetchXml)
    {
        var fetchExpression = new FetchExpression(fetchXml);
        var entities = service.RetrieveMultiple(fetchExpression);
        return entities.Entities;
    }

    static IOrganizationService ConnectToCrm(string connectionString)
    {
        CrmServiceClient service = new CrmServiceClient(connectionString);
        return service.OrganizationServiceProxy;
    }


    static void work2()
    {
        //Connection String: Replace the connection string details (Url, Username, Password) with your actual Dynamics 365 credentials.
        string connectionString = "AuthType=Office365; Url=https://yourorg.crm.dynamics.com; Username=yourusername; Password=yourpassword;";
        CrmServiceClient serviceClient = new CrmServiceClient(connectionString);
        if (serviceClient != null && serviceClient.IsReady)
        {
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
                string fileName = entity.GetAttributeValue<string>("filename");
                string encodedContent = entity.GetAttributeValue<string>("documentbody");

                if (!string.IsNullOrEmpty(encodedContent))
                {
                    byte[] fileContent = Convert.FromBase64String(encodedContent);
                    File.WriteAllBytes(@"C:\temp\" + fileName, fileContent);
                }
                
            }
        }
        else
        {
            Console.WriteLine("Connection not established");
        }
    }


    static void work1() {

        string connectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";
        //SqlConnection con = new SqlConnection(connectionString);
        // con.Open();
        var service = ConnectToCrm(connectionString);

        string fetchXml = @"<fetch mapping='logical'>
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

        var results = ExecuteFetchXml(service, fetchXml);
        var localDirectory = @"C:\temp\";

        foreach (var record in results)
        {
            string fileName = record["filename"].ToString();
            string encodedContent = record["documentbody"].ToString();
            string mimeType = record["mimetype"].ToString();

            // Proceed to decode and save the file
            byte[] fileContent = Convert.FromBase64String(encodedContent);
            File.WriteAllBytes(Path.Combine(localDirectory, fileName), fileContent);
        }
        

    }


    static void work()
    {
        string connectionString = "your_connection_string_here";
        string fetchXml = @"<fetch mapping='logical'>
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

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string sql = "SELECT annotationid, filename, documentbody, mimetype FROM Annotation WHERE FetchXmlMethodHere(@fetchXml)"; // Adjust this line based on how you can execute FetchXML in your context
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@fetchXml", fetchXml);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var fileName = reader["filename"].ToString();
                        var documentBody = reader["documentbody"].ToString();
                        var mimeType = reader["mimetype"].ToString();

                        if (!string.IsNullOrEmpty(documentBody))
                        {
                            byte[] fileBytes = Convert.FromBase64String(documentBody);
                            string directoryPath = Path.Combine(Environment.CurrentDirectory, "ExportedFiles");
                            Directory.CreateDirectory(directoryPath);
                            string filePath = Path.Combine(directoryPath, fileName);

                            File.WriteAllBytes(filePath, fileBytes);
                            Console.WriteLine($"Exported {fileName}");
                        }
                    }
                }
            }
        }
        Console.WriteLine("All files have been downloaded.");
    }
    /*
    Guid AttachToIncident(string filePath, Guid recordGuid)
    {
        Func<string, string> imageToBase64 = (fpath) => {
            using (Image image = Image.FromFile(fpath))
            {
                using (MemoryStream memStrm = new MemoryStream())
                {
                    image.Save(memStrm, image.RawFormat);
                    byte[] imageBytes = memStrm.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        };

        string fileName = Path.GetFileName(filePath);

        Guid attachmentId = Guid.Empty;

        Entity newAnnotation = new Entity("annotation");
        newAnnotation["subject"] = "external attachment";
        newAnnotation["filename"] = filename;
        newAnnotation["mimetype"] = @"image/jpeg";
        newAnnotation["documentbody"] = imageToBase64(filePath);
        newAnnotation["objectid"] = new EntityReference("incident", recordGuid);

        //you must be knowing what this service is ;)
        attachmentId = orgService.Create(newAnnotation);
        return attachmentId;
    }
    */
    static void OldMain()
    {
        bool useMockData = true; // testing bool

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

                // crm data
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

