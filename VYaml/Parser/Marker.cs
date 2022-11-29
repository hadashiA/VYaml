namespace VYaml.Parser
{
    public struct Marker
    {
        public int Position;
        public int Line;
        public int Col;

        public Marker(int position, int line, int col)
        {
            Position = position;
            Line = line;
            Col = col;
        }

        public override string ToString() => $"Line: {Line}, Col: {Col}, Idx: {Position}";
    }
}

