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

        private class CliOptions
        {
            [Option('p', "path", Required = true, HelpText = "The path to the on-disk directory holding DTDL models.")]
            public string ModelPath { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CliOptions>(args)
                   .WithParsed(o =>
                   {
                       modelPath = o.ModelPath;
                   });

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
                EnumerationOptions options = new EnumerationOptions() { RecurseSubdirectories = true };
                foreach (string file in Directory.EnumerateFiles(modelPath, "*.json", options))
                {
                    StreamReader r = new StreamReader(file);
                    string dtdl = r.ReadToEnd();
                    r.Close();
                    Response<ModelData[]> res = client.CreateModels(new List<string>() { dtdl });
                    Log.Ok($"Model {file.Split("\\").Last()} created successfully!");
                    foreach (ModelData md in res.Value)
                        LogResponse(md.Model);
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
