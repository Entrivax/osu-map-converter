using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Map.Converter.IntralismObjects
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Resource
    {
        [JsonProperty(PropertyName = "name")]
        public string Name;

        [JsonProperty(PropertyName = "type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceType Type;

        public enum ResourceType { Sprite }

        [JsonProperty(PropertyName = "path")]
        public string Path;

        public Resource(string name, ResourceType type, string path)
        {
            this.Name = name;
            this.Type = type;
            this.Path = path;
        }
    }
}
