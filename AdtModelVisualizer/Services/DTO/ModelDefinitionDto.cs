using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdtModelVisualizer.Services.DTO
{
    public class ModelDefinitionDto
    {
        private JsonElement? displayNameElement;
        private JsonElement? descriptionElement;
        private JsonElement? extendModelIdsElement;
        private JsonElement? contentsElement;

        [JsonPropertyName("@id")]
        public string Id { get; set; }

        [JsonPropertyName("@type")]
        public ModelDefinitionTypeDto Type { get; set; }

        [JsonPropertyName("@context")]
        public List<string> Context { get; set; }

        [JsonPropertyName("displayName")]
        public JsonElement? DisplayNameElement
        {
            get => displayNameElement;
            set 
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    displayNameElement = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("description")]
        public JsonElement? DescriptionElement {
            get => descriptionElement;
            set 
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    descriptionElement = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("extends")]
        public JsonElement? ExtendModelIdsElement { 
            get => extendModelIdsElement;
            set
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    extendModelIdsElement = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("contents")]
        public JsonElement? ContentsElement { 
            get => contentsElement;
            set
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    contentsElement = value.Value.Clone();
                }
            }
        }

        [JsonIgnore]
        public List<string> ExtendModelIds
        {
            get
            {
                if (ExtendModelIdsElement.HasValue)
                {
                    if (ExtendModelIdsElement.Value.ValueKind == JsonValueKind.String)
                    {
                        return new List<string> { ExtendModelIdsElement.Value.GetString() };
                    }
                    else if (ExtendModelIdsElement.Value.ValueKind == JsonValueKind.Array)
                    {
                        var output = new List<string>();
                        foreach (JsonElement extendElement in ExtendModelIdsElement.Value.EnumerateArray())
                        {
                            output.Add(extendElement.GetString());
                        }
                        return output;
                    }
                }
                return null;
            }
        }


        [JsonIgnore]
        public List<ModelDefinitionContentDto> Contents
        {
            get
            {
                var output = new List<ModelDefinitionContentDto>();
                if (ContentsElement.HasValue)
                {
                    if (ContentsElement.Value.ValueKind == JsonValueKind.Object)
                    {
                        output.Add(JsonSerializer.Deserialize<ModelDefinitionContentDto>(ContentsElement.ToString()));
                    }
                    else if (ContentsElement.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement relationship in ContentsElement.Value.EnumerateArray())
                        {
                            output.Add(JsonSerializer.Deserialize<ModelDefinitionContentDto>(relationship.ToString()));
                        }
                    }
                }

                return output;
            }
        }

        [JsonIgnore]
        public ModelDisplayNameDto DisplayName
        {
            get
            {
                if (DisplayNameElement.HasValue)
                {
                    if (DisplayNameElement.Value.ValueKind == JsonValueKind.String)
                    {
                        return new ModelDisplayNameDto { En = DisplayNameElement.Value.GetString() };
                    }
                    else if (DisplayNameElement.Value.ValueKind == JsonValueKind.Object)
                    {
                        return new ModelDisplayNameDto { En = DisplayNameElement.Value.GetProperty("en").GetString() };
                    }
                }
                return null;
            }
        }

        [JsonIgnore]
        public ModelDescriptionDto Description
        {
            get
            {
                if (DescriptionElement.HasValue)
                {
                    if (DescriptionElement.Value.ValueKind == JsonValueKind.String)
                    {
                        return new ModelDescriptionDto { En = DescriptionElement.Value.GetString() };
                    }
                    else if (DisplayNameElement.Value.ValueKind == JsonValueKind.Object)
                    {
                        return new ModelDescriptionDto { En = DescriptionElement.Value.GetProperty("en").GetString() };
                    }
                }
                return null;
            }
        }


        public static ModelDefinitionDto MapFromModelData(string model)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return JsonSerializer.Deserialize<ModelDefinitionDto>(model, options);
        }
    }
}