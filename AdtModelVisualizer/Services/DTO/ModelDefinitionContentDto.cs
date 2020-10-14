using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdtModelVisualizer.Services.DTO
{
    public class ModelDefinitionContentDto
    {
        private JsonElement? type;
        private JsonElement? displayNameElement;
        private JsonElement? descriptionElement;
        private JsonElement? schema;

        [JsonPropertyName("@type")]
        public JsonElement? Type
        {
            get => type;
            set
            {
                if (value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    type = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("target")]
        public string Target { get; set; }

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
        public JsonElement? DescriptionElement
        {
            get => descriptionElement;
            set
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    descriptionElement = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("schema")]
        public JsonElement? Schema
        {
            get => schema;
            set
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    schema = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }

        public bool HasType(ModelDefinitionContentTypeDto type)
        {
            if (this.type.HasValue)
            {
                if (this.type.Value.ValueKind == JsonValueKind.String)
                {
                    Enum.TryParse(this.Type.Value.GetString(), true, out ModelDefinitionContentTypeDto parsedType);
                    return parsedType == type;
                }
                else if (this.Type.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in this.Type.Value.EnumerateArray())
                    {
                        Enum.TryParse(item.GetString(), true, out ModelDefinitionContentTypeDto parsedType);
                        if (parsedType == type)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                throw new Exception($"Unknown content type: {Type.Value.ValueKind} {Type.Value}");
            }
            throw new Exception($"Unknown content type: Type is null");
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
    }
}