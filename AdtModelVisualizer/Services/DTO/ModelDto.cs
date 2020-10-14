using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AdtModelVisualizer.Services.DTO
{
    public class ModelDto
    {
        [JsonPropertyName("displayName")]
        public ModelDisplayNameDto DisplayName { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("description")]
        public ModelDescriptionDto Description { get; set; }

        [JsonPropertyName("uploadTime")]
        public DateTimeOffset? UploadTime { get; set; }

        [JsonPropertyName("decommissioned")]
        public bool? Decommissioned { get; set; }

        [JsonPropertyName("model")]
        public ModelDefinitionDto ModelDefinition { get; set; }

        public static List<ModelDto> MapFromModelData(List<ModelData> models)
        {
            return models?.Select(MapFromModelData).ToList();
        }

        public static ModelDto MapFromModelData(ModelData model)
        {
            return new ModelDto
            {
                Decommissioned = model.Decommissioned,
                Description = (model.Description == null || !model.Description.ContainsKey("en")) ? null : new ModelDescriptionDto { En = model.Description["en"] },
                DisplayName = (model.DisplayName == null || !model.DisplayName.ContainsKey("en")) ? null : new ModelDisplayNameDto { En = model.DisplayName["en"] },
                Id = model.Id,
                UploadTime = model.UploadTime,
                ModelDefinition = ModelDefinitionDto.MapFromModelData(model.Model)
            };
        }
    }

    public class ModelDisplayNameDto
    {
        [JsonPropertyName("en")]
        public string En { get; set; }
    }

    public class ModelDescriptionDto
    {
        [JsonPropertyName("en")]
        public string En { get; set; }
    }
}