using System;
using System.Runtime.CompilerServices;

namespace DifferentMethods.FuzzBall
{
    [System.Serializable]
    public class DelayLine : RackItem<DelayLine>
    {
        [SignalRange(0, 10)]
        public Signal delay = new Signal(0.5f);
        [SignalRange(0, 1)]
        public Signal feedback = new Signal(0f);
        [SignalRange(0, 1)]
        public Signal amp = new Signal(1);
        [NonSerialized] public Signal output = new Signal();

        [NonSerialized] public Signal input = new Signal();
        [NonSerialized] float[] buffer = new float[44100 * 10];

        public override void OnAddToRack(Synthesizer synth)
        {
            output.id = synth.NextOutputID();
        }

        public override string ToString()
        {
            return $"DelayLine: DELAY[{delay}] FEEDBACK[{feedback}] AMP[{amp}]";
        }

        public DelayLine(float delay, float feedback, float amp)
        {
            this.delay.localValue = delay;
            this.feedback.localValue = feedback;
            this.amp.localValue = amp;
        }


        public override void UpdateControl(float[] signals)
        {
            if (this.control == null) return;
            SyncControlSignal(signals, ref delay, ref control.delay);
            SyncControlSignal(signals, ref feedback, ref control.feedback);
            SyncControlSignal(signals, ref amp, ref control.amp);
        }


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

            buffer[index] = inp + (smp * feedback.GetValue(signals));
            index++;
            if (index >= bufferLength) index -= bufferLength;

            output.SetValue(signals, (smp * amp.GetValue(signals)));
        }

        [System.NonSerialized] float time = 0;
        [System.NonSerialized] int index = 0;

    }
}