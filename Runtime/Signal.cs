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
            var value = (id == 0) ? localValue : signals[id];
            if (float.IsNaN(value)) value = 0;
            return value;
        }

        public void SetValue(float[] signals, float value)
        {
            // if (float.IsNaN(value)) value = 0;
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