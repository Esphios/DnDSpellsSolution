using Newtonsoft.Json;

namespace ApplicationCore.Dtos
{
    public class SpellAPIResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("results")]
        public List<SpellSummary>? Results { get; set; }
    }
}
