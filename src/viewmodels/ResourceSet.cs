using Newtonsoft.Json;

namespace Alexa.BridgeBot.Lambda.viewmodels
{
    public partial class ResourceSet
    {
        [JsonProperty("estimatedTotal")]
        public long EstimatedTotal { get; set; }

        [JsonProperty("resources")]
        public Resource[] Resources { get; set; }
    }
}
