using osu.Map.Converter.Utils;
using System.Collections.Generic;

namespace osu.Map.Converter.StepmaniaObjects
{
    class Simfile
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Artist { get; set; }
        public string TitleTranslit { get; set; }
        public string SubTitleTranslit { get; set; }
        public string ArtistTranslit { get; set; }
        public string Credit { get; set; }
        public string Banner { get; set; }
        public string Background { get; set; }
        public string CDTitle { get; set; }
        public string Music { get; set; }
        public string MapCreator { get; set; }
        public string Comments { get; set; }

        public float Offset { get; set; }
        public float SampleStart { get; set; }
        public float SampleLength { get; set; }

        public bool Selectable { get; set; }
        public List<DoublePair> BPMS { get; set; }
        public List<DoublePair> Stops { get; set; }
        public List<Measure> Notes { get; set; }

        public Simfile()
        {
            Title = SubTitle = Artist = TitleTranslit = SubTitleTranslit = ArtistTranslit = Credit = Banner = Background = CDTitle = Music = "";
            Offset = SampleStart = SampleLength = 0;
            Selectable = true;
            BPMS = new List<DoublePair>();
            Stops = new List<DoublePair>();
            Notes = new List<Measure>();
        }
    }
}
