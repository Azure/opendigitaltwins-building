using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AdtModelVisualizer.GraphModels
{
    public class Node
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }
    }

    public class Edge
    {
        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("color")]
        public EdgeColor Color { get; set; }
    }

    public class EdgeColor
    {
        [JsonPropertyName("color")]
        public string Color { get; set; }
    }

    public class Graph
    {
        [JsonPropertyName("nodes")]
        public List<Node> Nodes { get; set; }

        [JsonPropertyName("edges")]
        public List<Edge> Edges { get; set; }
    }
}