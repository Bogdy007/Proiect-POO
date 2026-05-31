using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Story.Core.Models
{
    public class Poveste
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("startBlock")]
        public string StartBlock { get; set; } = "";

        [JsonPropertyName("properties")]
        public List<AtributPoveste> Attributes { get; set; } = new List<AtributPoveste>();

        [JsonPropertyName("blocks")]
        public List<BlocPoveste> Blocks { get; set; } = new List<BlocPoveste>();
    }

    public class AtributPoveste
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("hudLabel")]
        public string HudLabel { get; set; }

        [JsonPropertyName("min")]
        public int Min { get; set; }

        [JsonPropertyName("max")]
        public int Max { get; set; }

        [JsonPropertyName("initial")]
        public int Initial { get; set; }

        [JsonPropertyName("visibleInHud")]
        public bool VisibleInHud { get; set; }

        [JsonPropertyName("hudOrder")]
        public int HudOrder { get; set; }

        [JsonPropertyName("onMinBlock")]
        public string MinBlock { get; set; }

        [JsonPropertyName("onMaxBlock")]
        public string MaxBlock { get; set; }
    }
