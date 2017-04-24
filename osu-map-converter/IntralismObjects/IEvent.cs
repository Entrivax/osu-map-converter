using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Map.Converter.IntralismObjects
{
    [JsonObject(MemberSerialization.OptIn)]
    public interface IEvent
    {
        [JsonProperty(PropertyName = "time")]
        float Time { get; set; }

        [JsonProperty(PropertyName = "data")]
        string[] Data { get; }
    }
}
