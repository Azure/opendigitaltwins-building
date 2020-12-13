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
using System.Text.RegularExpressions;

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
                // If -d Option specified Delete All Models first
                if (deleteFirst)
                    DeleteAllModels(1);

                // Go over directories
                EnumerationOptions options = new EnumerationOptions() { RecurseSubdirectories = true };
                foreach (string file in Directory.EnumerateFiles(modelPath, "*.json", options))
                {
                    UploadModel(file);
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

        private static bool UploadModel(string file)
        {
            bool exitProcess = false;
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

                        // first inspect Extends Section
                        exitProcess = ProcessExtendsSection(file, dtdl);
                        if (exitProcess) return true;

                        exitProcess = ProcessPropertiesSection(file, dtdl);
                        if (exitProcess) return true;

                        // now try the original file back again
                        exitProcess = UploadModel(file);

                        break;
                    default:
                        break;
                }
            }
            return exitProcess;
        }

        private static bool ProcessExtendsSection(string file, string dtdl)
        {
            bool exitProcess = false;
            MatchCollection extendsSection = Regex.Matches(dtdl, "extends[\" :[\r\na-zA-Z0-9;_,]*]", RegexOptions.Singleline);
            if (extendsSection.Count > 0)
            {
                string extendsSectionFull = extendsSection[0].Value;
                MatchCollection dtmiExtends = Regex.Matches(extendsSectionFull, "dtmi:[ :[a-zA-Z0-9;_]*", RegexOptions.Singleline);

                foreach (Match dtmiExtendsMatch in dtmiExtends)
                {
                    // find this model
                    try
                    {
                        Response<ModelData> res = client.GetModel(dtmiExtendsMatch.Value);
                        // model found! keep going
                    }
                    catch (RequestFailedException e)
                    {
                        // Model Not Found - find it in the directory and call Upload Model
                        if (e.Status == 404)
                        {
                            string missingInterface = dtmiExtendsMatch.Value;
                            int missingPartIndex = missingInterface.LastIndexOf(":");
                            missingInterface = missingInterface.Substring(missingPartIndex + 1).Replace(";1", "");
                            missingInterface = missingInterface.Replace(";", "");

                            string[] missingFile = Directory.GetFiles(modelPath, missingInterface + ".json*", SearchOption.AllDirectories);
                            if (missingFile.Count<string>() == 0)
                            {
                                // no file found, perhaps the definition of the schema is contained within the current file, lets try
                                exitProcess = UploadModel(file);
                                if (exitProcess == true)
                                {
                                    Log.Error($"Could not find a definition for Interace {" + missingInterface + "}");
                                }
                            }
                            else {  
                                // try to Upload the Model in the extends section
                                exitProcess = UploadModel(missingFile[0]);
                            }
                        }
                        else
                        {
                            Log.Error($"Error in extends section in Model {file.Split("\\").Last()}");
                            Log.Error($"Response {e.Status}: {e.Message}");
                            exitProcess = true;
                        }
                    }
                }
            }
            else
            {
                Log.Error($"extends section not found in Model {file.Split("\\").Last()}!");
                exitProcess = true;
            }
            return exitProcess;
        }

        private static bool ProcessPropertiesSection(string file, string dtdl)
        {
            bool exitProcess = false;
            // Model could not be uploaded because of a dependency
            
            MatchCollection matches = Regex.Matches(dtdl.Replace(" ", ""), "\"schema\":\"dtmi:.*;");            
            // first find if there are multiple other references
            foreach (Match match in matches)
            {
                // find the missing dependency Model in the message
                string missingInterface = match.ToString();
                int missingPartIndex = missingInterface.LastIndexOf(":");
                missingInterface = missingInterface.Substring(missingPartIndex + 1).Replace(";", "");

                string[] missingFile = Directory.GetFiles(modelPath, missingInterface + ".json*", SearchOption.AllDirectories);
                if (missingFile.Count<string>() == 0)
                {
                    // no file found, perhaps the definition of the schema is contained within the current file, lets try
                    exitProcess = UploadModel(file);
                    if (exitProcess == true)
                    {
                        Log.Error($"Could not find a definition for Interace {" + missingInterface + "}");
                    }
                }
                else
                {
                    // try to Upload the Model that is referred in the Properties section
                    exitProcess = UploadModel(missingFile[0]);
                }
            }
            return exitProcess;
        }

        private static void DeleteAllModels(int iteration)
        {
            IEnumerable<ModelData> ie = client.GetModels() as IEnumerable<ModelData>;
            int count = ie.Count<ModelData>();

            foreach (ModelData md in client.GetModels())
            {
                try
                {
                    client.DeleteModel(md.Id);
                    Log.Ok("Successfully deleted Model {" + md.Id + "}. Attempt [" + iteration + "]");
                }
                catch (RequestFailedException e2)
                {
                    //Log.Error("Failed to delete Model {" + md.Id + "}");
                    //Log.Error(e2.Message);
                    // skip this and go to the next one
                }
            }

            try
            {
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