using System;
using System.Runtime.CompilerServices;

namespace DifferentMethods.FuzzBall
{
    public class InternalDelayLine
    {
        float[] buffer;
        int position;

        public int Length
        {
            get { return buffer.Length; }
        }

        public float Last
        {
            get { return buffer[position]; }
        }

        public InternalDelayLine(int length)
        {
            buffer = new float[length];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Tick(float input)
        {
            var last = buffer[position];
            buffer[position] = input;
            if (++position >= (buffer.Length))
            {
                position = 0;
            }
            return last;
        }

    }
}