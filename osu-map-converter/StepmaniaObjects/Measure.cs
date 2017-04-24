namespace osu.Map.Converter.StepmaniaObjects
{
    public class Measure
    {
        private Note[] _notes = new Note[192];

        public bool IsNoteNull(int note)
        {
            return _notes[note] == null;
        }

        public Note this[int i] { get { if (_notes[i] == null) _notes[i] = new Note(); return _notes[i]; } }

        public int MostAdaptedNumberOfLines
        {
            get
            {
                /*
                int mostAdapted = 4;
                for (int i = 0; i < 192; i++)
                {
                    bool haveNote = false;
                    for (int j = 0; j < 4; j++)
                    {
                        if (_notes[i][j] != '0')
                        {
                            haveNote = true;
                            break;
                        }
                    }
                    if (haveNote)
                    {
                        if (i % 4 != 0)
                        {
                            if (mostAdapted < 4)
                                mostAdapted = 4;
                        }
                        else if (i % 8 != 0)
                        {
                            if (mostAdapted)
                        }
                    }
                }
                /**/
                return 192;
            }
        }
    }
}
