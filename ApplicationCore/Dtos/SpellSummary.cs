using Newtonsoft.Json;

namespace ApplicationCore.Dtos
{
    public class SpellSummary
    {
        [JsonProperty("index")]
        public string? Index { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("level")]
        public int? Level { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }
    }

}
