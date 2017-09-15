using Newtonsoft.Json;

namespace Alexa.BridgeBot.Lambda.viewmodels
{
    public partial class Resource
    {
        [JsonProperty("incidentId")]
        public long IncidentId { get; set; }

        [JsonProperty("severity")]
        public long Severity { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("roadClosed")]
        public bool RoadClosed { get; set; }

        [JsonProperty("detour")]
        public string Detour { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }

        [JsonProperty("source")]
        public long Source { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }
    }

}
