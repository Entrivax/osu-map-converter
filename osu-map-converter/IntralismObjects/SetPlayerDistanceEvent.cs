using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Map.Converter.IntralismObjects
{
    public class SetPlayerDistanceEvent : IEvent
    {
        public float Time { get; set; }

        public string[] Data
        {
            get
            {
                return new string[]
                {
                    "SetPlayerDistance",
                    $"{Distance}"
                };
            }
        }

        public int Distance;
    }
}
