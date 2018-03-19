using System;
using System.Runtime.CompilerServices;

namespace DifferentMethods.FuzzBall
{
    [System.Serializable]
    public class CombFilter : RackItem<CombFilter>
    {
        [SignalRange(0, 1)]
        public Signal delay = new Signal(0.5f);

        [NonSerialized] public Signal input = new Signal();
        [NonSerialized] public Signal output = new Signal();
        [NonSerialized] float[] buffer = new float[44100 * 1];

        public override void OnAddToRack(Synthesizer synth)
        {
            output.id = synth.NextOutputID();
        }

        public CombFilter(float delay)
        {
            this.delay.localValue = delay;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateControl(float[] signals)
        {
            if (this.control == null) return;
            SyncControlSignal(signals, ref delay, ref control.delay);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Tick(float[] signals)
        {
            var bufferLength = buffer.Length;
            var timeTarget = delay.GetValue(signals) * SAMPLERATE;
            time = 0.0001f * timeTarget + 0.9999f * time;
            var samples = (int)time;
            var frac = time - samples;
            var pastIndex = index - samples;
            while (pastIndex < 0) pastIndex = bufferLength + pastIndex;
            var A = buffer[pastIndex];
            var B = buffer[(pastIndex + 1) % bufferLength];
            var smp = (B - A) * frac + A;
            var inp = input.GetValue(signals);
            buffer[index] = inp;
            index++;
            if (index >= bufferLength) index -= bufferLength;
            output.SetValue(signals, smp + inp);
        }

        [System.NonSerialized] float time = 0;
        [System.NonSerialized] int index = 0;

    }
}