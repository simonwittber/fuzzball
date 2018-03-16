namespace DifferentMethods.FuzzBall
{
    public struct NoteTrigger : System.IComparable<NoteTrigger>
    {
        public float hz;
        public float volume;
        public int duration;
        public int beat;
        public int noteNumber;

        public int CompareTo(NoteTrigger other)
        {
            return beat.CompareTo(other.beat);
        }

        public override string ToString()
        {
            return $"B{beat} - {hz}:{duration}:{volume}";
        }
    }

}