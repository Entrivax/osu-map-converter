using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Map.Converter.IntralismObjects
{
    public class SetBGColorEvent : IEvent
    {
        public float Time { get; set; }
 
        public string[] Data
        {
            get
            {
                return new string[]
                {
                    "SetBGColor",
                    $"[{Color.R},{Color.G},{Color.B},{Speed}]"
                };
            }
        }

        public Color Color;

        public int Speed;

        public enum Object { Up, Left, Right, Down };
    }
}
