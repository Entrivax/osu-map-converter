namespace osu.Map.Converter.StepmaniaObjects
{
    public class Note
    {
        private char[] _columns;

        public char this[int i] { get { return _columns[i]; } set { _columns[i] = value; } }

        public Note()
        {
            _columns = new char[] { '0', '0', '0', '0' };
        }
    }
}