using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Alexa.BridgeBot.Lambda.viewmodels
{
    public partial class TrafficEvent
    {
        [JsonProperty("resourceSets")]
        public ResourceSet[] ResourceSets { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty("statusCode")]
        public long StatusCode { get; set; }
    }

}
