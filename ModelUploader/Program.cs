using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using CommandLine;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ModelUploader
{
    public class Program
    {
        // Properties to establish connection
        // Please copy the file serviceConfig.json.TEMPLATE to serviceConfig.json 
        // and set up these values in the config file
        private static string clientId;
        private static string tenantId;
        private static string adtInstanceUrl;

        private static DigitalTwinsClient client;
        private static string modelPath;
        private static bool deleteFirst;

        private class CliOptions
        {
            [Option('p', "path", Required = true, HelpText = "The path to the on-disk directory holding DTDL models.")]
            public string ModelPath { get; set; }
            [Option('d', "deletefirst", Required = false, HelpText = "Specify if you want to delete the models first, by default is false")]
            public bool DeleteFirst { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CliOptions>(args)
                   .WithParsed(o =>
                   {
                       modelPath = o.ModelPath;
                       deleteFirst = o.DeleteFirst;
                   }                   
                   );

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                int width = Math.Min(Console.LargestWindowWidth, 150);
                int height = Math.Min(Console.LargestWindowHeight, 40);
                Console.SetWindowSize(width, height);
            }

            try
            {
                // Read configuration data from the 
                IConfiguration config = new ConfigurationBuilder()
                    .AddJsonFile("serviceConfig.json", false, true)
                    .Build();
                clientId = config["clientId"];
                tenantId = config["tenantId"];
                adtInstanceUrl = config["instanceUrl"];
            }
            catch (Exception)
            {
                Log.Error($"Could not read service configuration file serviceConfig.json");
                Log.Alert($"Please copy serviceConfig.json.TEMPLATE to serviceConfig.json");
                Log.Alert($"and edit to reflect your service connection settings.");
                Log.Alert($"Make sure that 'Copy always' or 'Copy if newer' is set for serviceConfig.json in VS file properties");
                Environment.Exit(0);
            }

            Log.Ok("Authenticating...");
            try
            {
                var credential = new InteractiveBrowserCredential(tenantId, clientId);
                client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);
                // force authentication to happen here
                try
                {
                    client.GetDigitalTwin("---");
                }
                catch (RequestFailedException)
                {
                    // As we are intentionally try to retrieve a twin that is most likely not going to exist, this exception is expected
                    // We just do this to force the authentication library to authenticate ahead
                }
                catch (Exception e)
                {
                    Log.Error($"Authentication or client creation error: {e.Message}");
                    Log.Alert($"Have you checked that the configuration in serviceConfig.json is correct?");
                    Environment.Exit(0);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Authentication or client creation error: {e.Message}");
                Log.Alert($"Have you checked that the configuration in serviceConfig.json is correct?");
                Environment.Exit(0);
            }

            Log.Ok($"Service client created – ready to go");
            try
            {
                // Delete All Models first
                if (deleteFirst)
                    DeleteAllModels(1);                

                EnumerationOptions options = new EnumerationOptions() { RecurseSubdirectories = true };
                foreach (string file in Directory.EnumerateFiles(modelPath, "*.json", options))
                {
                    StreamReader r = new StreamReader(file);
                    string dtdl = r.ReadToEnd();
                    r.Close();

                    try
                    {
                        Response<ModelData[]> res = client.CreateModels(new List<string>() { dtdl });
                        Log.Ok($"Model {file.Split("\\").Last()} created successfully!");
                        foreach (ModelData md in res.Value)
                            LogResponse(md.Model);
                    }
                    catch (RequestFailedException e)
                    {
                        switch (e.Status)
                        {
                            case 409:
                                // 409 is when the Model already exists - so just skip this model
                                Log.Ok($"Model {file.Split("\\").Last()} already exists, skipped!");
                                break;
                            case 400:
                                // Model could not be uploaded because of a dependency

                                // find the missing dependency Model in the message
                                int missingInterfaceIndexStart = e.Message.IndexOf("dtmi:");
                                int missingInterfaceIndexEnd = e.Message.IndexOf(";1", missingInterfaceIndexStart);
                                string missingInterface = e.Message.Substring(missingInterfaceIndexStart, missingInterfaceIndexEnd - missingInterfaceIndexStart);
                                int missingPartIndex = missingInterface.LastIndexOf(":");
                                missingInterface = missingInterface.Substring(missingPartIndex + 1);

                                string[] missingFile = Directory.GetFiles(modelPath, missingInterface + ".json*", SearchOption.AllDirectories);
                                if (missingFile[0] == null || missingFile[0] == "") break;

                                // Read the contents of the file
                                StreamReader r1 = new StreamReader(missingFile[0]);
                                string dtdlDependency = r1.ReadToEnd();
                                r1.Close();

                                try
                                {
                                    // Upload the dependency model 
                                    Response<ModelData[]> res1 = client.CreateModels(new List<string>() { dtdlDependency });
                                    Log.Ok($"Dependent Model {file.Split("\\").Last()} created successfully! Now proceeeding with the original model");
                                    foreach (ModelData md1 in res1.Value)
                                        LogResponse(md1.Model);
                                }
                                catch (RequestFailedException e1)
                                {
                                    // dependency file didnt work, bail
                                    Log.Error($"Dependency Model {missingFile[0].Split("\\").Last()}");
                                    Log.Error($"Response {e1.Status}: {e1.Message}");

                                    Log.Error($"Original Model We were trying for {file.Split("\\").Last()}");
                                    return;
                                }

                                // now try the original file back again
                                try
                                {
                                    Response<ModelData[]> res = client.CreateModels(new List<string>() { dtdl });
                                    Log.Ok($"Model {file.Split("\\").Last()} created successfully!");
                                    foreach (ModelData md in res.Value)
                                        LogResponse(md.Model);
                                }
                                catch (RequestFailedException e2)
                                {
                                    // didnt work again, bail
                                    Log.Error($"Model {file.Split("\\").Last()}");
                                    Log.Error($"Response {e2.Status}: {e2.Message}");
                                    return;
                                }

                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (RequestFailedException e)
            {
                Log.Error($"Response {e.Status}: {e.Message}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex.Message}");
            }
        }

        private static void DeleteAllModels(int iteration)
        {
            IEnumerable<ModelData> ie = client.GetModels() as IEnumerable<ModelData>;
            int count = ie.Count<ModelData>();

            foreach (ModelData md in client.GetModels())
            {
                try {  
                    client.DeleteModel(md.Id);
                    Log.Ok("Successfully deleted Model {" + md.Id + "}. Attempt [" + iteration + "]");
                }
                catch (RequestFailedException e2)
                {
                    Log.Error("Failed to delete Model {" + md.Id + "}");
                    //Log.Error(e2.Message);
                    // skip this and go to the next one
                }
            }

            try {
                IEnumerable<ModelData> c = client.GetModels() as IEnumerable<ModelData>;
                if (c.Count<ModelData>() > 0) DeleteAllModels(iteration + 1);
            }
            catch (Exception)
            {
                return;
            }
        }

        private static void LogResponse(string res, string type = "")
        {
            if (type != "")
                Log.Alert($"{type}: \n");
            else
                Log.Alert("Response:");

            if (res == null)
                Log.Out("Null response");
            else
                Console.WriteLine(PrettifyJson(res));
        }

        private static string PrettifyJson(string json)
        {
            object jsonObj = JsonSerializer.Deserialize<object>(json);
            return JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
