using System;

namespace DifferentMethods.FuzzBall
{
    [Serializable]
    public struct Signal
    {
        public int id;
        public float localValue;

        public override string ToString()
        {
            if (id == 0)
                return $"[{localValue.ToString("F2")}]";
            return $"[#{id}]";
        }

        public Signal(float localValue) : this()
        {
            this.localValue = localValue;
        }

        public float GetValue(float[] signals)
        {
            if (id == 0) return localValue;
            return signals[id];
        }

        public void SetValue(float[] signals, float value)
        {
            if (id == 0)
                localValue = value;
            else
                signals[id] = value;
        }

        internal void Connect(Signal output)
        {
            this.id = output.id;
        }
    }

}