using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AdtModelVisualizer.Services;
using AdtModelVisualizer.Services.DTO;
using AdtModelVisualizer.GraphModels;
using Microsoft.AspNetCore.Mvc;

namespace AdtModelVisualizer.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AdtModelsController : ControllerBase
    {
        private readonly IAdtApiService _apiService;

        public AdtModelsController(IAdtApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet("api/graph")]
        public IActionResult GetGraph()
        {
            var models = _apiService.GetModels();
            var edges = new List<Edge>();
            foreach (var model in models)
            {
                if (model.ModelDefinition.Contents != null)
                {
                    var relationships = model.ModelDefinition.Contents.Where(x => x.HasType(ModelDefinitionContentTypeDto.Relationship));
                    edges.AddRange(relationships.Select(x => new Edge
                    {
                        From = model.Id,
                        To = x.Target,
                        Label = x.Name
                    }));
                }
                if (model.ModelDefinition.ExtendModelIds != null)
                {
                    edges.AddRange(model.ModelDefinition.ExtendModelIds.Select(x => new Edge
                    {
                        From = model.Id,
                        To = x,
                        Label = "extends",
                        Color = new EdgeColor { Color = "green" }
                    }));
                }
            }
            var graph = new Graph
            {
                Nodes = models.Where(x => x.ModelDefinition.Type == ModelDefinitionTypeDto.Interface)
                              .Select(x => new Node { Id = x.Id, Label = x.DisplayName?.En, Title = x.Id })
                              .ToList(),
                Edges = edges
            };
            return Ok(graph);
        }

        [HttpGet("api/nodes/{nodeId}")]
        public IActionResult GetNode(string nodeId)
        {
            var settings = new JsonSerializerOptions();
            settings.IgnoreNullValues = true;
            settings.IgnoreReadOnlyProperties = true;
            var model = _apiService.GetModel(nodeId);
            var node = new Node
            {
                Id = model.Id,
                Label = model.DisplayName.En,
                Title = model.Id,
                Data = JsonSerializer.Serialize(model.ModelDefinition, settings)
            };
            return Ok(node);
        }

        [HttpGet("api/models")]
        public IActionResult GetModels()
        {
            var models = _apiService.GetModels();
            return Ok(models.Select(x => x.ModelDefinition));
        }

    }
}
