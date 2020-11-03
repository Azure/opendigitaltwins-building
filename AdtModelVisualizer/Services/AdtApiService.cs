using AdtModelVisualizer.Services.DTO;
using Azure.DigitalTwins.Core;
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
            _digitalTwinsClient = new DigitalTwinsClient(new Uri(configuration["AdtApiUrl"]), new AdtApiTokenCredential(tokenService));
        }

        public List<ModelDto> GetModels()
        {
            var modelData = _digitalTwinsClient.GetModels(includeModelDefinition: true).ToList();

            return ModelDto.MapFromModelData(modelData);
        }

        public List<string> GetModelsRaw()
        {
            var modelData = _digitalTwinsClient.GetModels(includeModelDefinition: true).ToList();
            return modelData.Select(m => m.Model).ToList();
        }


        public ModelDto GetModel(string modelId)
        {
            var modelDatum = _digitalTwinsClient.GetModel(modelId);
            return ModelDto.MapFromModelData(modelDatum.Value);
        }
    }
}