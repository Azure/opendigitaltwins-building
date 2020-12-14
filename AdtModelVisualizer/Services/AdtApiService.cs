using AdtModelVisualizer.Services.DTO;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdtModelVisualizer.Services
{
    public interface IAdtApiService
    {
        //Using SDK DigitalTwinsClient
        List<ModelDto> GetModels();
        ModelDto GetModel(string modelId);
    }

    public class AdtApiService : IAdtApiService
    {
        private readonly DigitalTwinsClient _digitalTwinsClient;

        public AdtApiService(IConfiguration configuration, ITokenService tokenService)
        {
            if(string.IsNullOrEmpty(configuration["UseMsiCredential"]) || Convert.ToBoolean(configuration["UseMsiCredential"]) == true)
            {
                DefaultAzureCredential cred = new DefaultAzureCredential(
                    new DefaultAzureCredentialOptions { ManagedIdentityClientId = "https://digitaltwins.azure.net"}
                    );
                _digitalTwinsClient = new DigitalTwinsClient(new Uri(configuration["AdtApiUrl"]), cred);
            }
            else
            {
                _digitalTwinsClient = new DigitalTwinsClient(new Uri(configuration["AdtApiUrl"]), new AdtApiTokenCredential(tokenService));
            }
        }

        public List<ModelDto> GetModels()
        {
            var modelData = _digitalTwinsClient.GetModels(new GetModelsOptions { IncludeModelDefinition = true }).ToList();

            return ModelDto.MapFromModelData(modelData);
        }

        public List<string> GetModelsRaw()
        {
            var modelData = _digitalTwinsClient.GetModels(new GetModelsOptions { IncludeModelDefinition = true }).ToList();
            return modelData.Select(m => m.DtdlModel).ToList();
        }


        public ModelDto GetModel(string modelId)
        {
            var modelDatum = _digitalTwinsClient.GetModel(modelId);
            return ModelDto.MapFromModelData(modelDatum.Value);
        }
    }
}