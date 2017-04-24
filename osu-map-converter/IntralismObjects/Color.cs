using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Map.Converter.IntralismObjects
{
    public class Color
    {
        private float r;
        public float R { get { return r; } set { if (value < 0) r = 0; else if (value > 1) r = 1; else r = value; } }
        private float g;
        public float G { get { return g; } set { if (value < 0) g = 0; else if (value > 1) g = 1; else g = value; } }
        private float b;
        public float B { get { return b; } set { if (value < 0) b = 0; else if (value > 1) b = 1; else b = value; } }
    }
}
