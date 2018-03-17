using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{

    [System.Serializable]
    public class Osc : RackItem<Osc>
    {
        public OscType type;
        [SignalRange(0, 22050)]
        public Signal freq = new Signal(440);
        [SignalRange(0, 22050)]
        public Signal detune = new Signal(0);
        [SignalRange(0, 2)]
        public Signal duty = new Signal(0.25f);
        [SignalRange(0, 1)]
        public Signal amp = new Signal(1);
        public Signal bias = new Signal(0);
        [NonSerialized] public Signal chain;
        [NonSerialized] public Signal output;

        public AnimationCurve shape = AnimationCurve.Linear(0, -1, 1, 1);

        [NonSerialized] float phase = 0, chainAmp = 1, localAmp = 0;



        public override string ToString()
        {
            return $"OSC: {type} F{freq} A{amp} B{bias} DU{duty} DE{detune} OUT:{output}";
        }

        public Osc ChainOutput(Signal output, float amp = 1f)
        {
            chain.id = output.id;
            chainAmp = amp;
            return this;
        }

        public Osc(OscType type, float freq, float amp, float detune, float bias, float duty)
        {
            this.type = type;
            this.freq.localValue = freq;
            this.amp.localValue = amp;
            this.detune.localValue = detune;
            this.bias.localValue = bias;
            this.duty.localValue = duty;
        }

        public override void OnAddToRack(Synthesizer synth)
        {
            output.id = synth.NextOutputID();
            if (type == OscType.Noise)
                BuildNoiseBuffer();
        }

        private void BuildNoiseBuffer()
        {
            noiseBuffer = new float[SAMPLERATE * 10];
            for (var i = 0; i < noiseBuffer.Length; i++)
                noiseBuffer[i] = Mathf.Lerp(-1, 1, Entropy.Next());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateControl(float[] signals)
        {
            if (control == null) return;
            type = control.type;
            SyncControlSignal(signals, ref freq, ref control.freq);
            SyncControlSignal(signals, ref detune, ref control.detune);
            SyncControlSignal(signals, ref duty, ref control.duty);
            SyncControlSignal(signals, ref amp, ref control.amp);
            SyncControlSignal(signals, ref bias, ref control.bias);
            if (this.shape == null && control.shape != null)
                this.shape = control.shape;
            else
                this.shape.keys = control.shape.keys;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Tick(float[] signals)
        {
            if (type == OscType.Noise && noiseBuffer == null) BuildNoiseBuffer();
            localAmp = Lerp(localAmp, amp.GetValue(signals), 0.01f);
            var smp = bias.GetValue(signals) + BandLimit(Sample(signals)) * localAmp;
            smp += chain.GetValue(signals) * chainAmp;
            output.SetValue(signals, smp);
            phase = phase + ((TWOPI * (freq.GetValue(signals) + detune.GetValue(signals))) / SAMPLERATE);
            if (phase > TWOPI)
                phase -= TWOPI;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float Sample(float[] signals)
        {
            switch (type)
            {
                case OscType.Sin:
                    return Mathf.Sin(phase);
                case OscType.WaveShape:
                    return shape.Evaluate(phase / TWOPI);
                case OscType.Square:
                    return phase < Mathf.PI ? 1f : -1f;
                case OscType.PWM:
                    return phase < Mathf.PI * duty.GetValue(signals) ? 1f : -1f;
                case OscType.Tan:
                    return Mathf.Clamp(Mathf.Tan(phase / 2), -1, 1);
                case OscType.Saw:
                    return 1f - (1f / Mathf.PI * phase);
                case OscType.Triangle:
                    if (phase < Mathf.PI)
                        return -1f + (2 * 1f / Mathf.PI) * phase;
                    else
                        return 3f - (2 * 1f / Mathf.PI) * phase;
                case OscType.Noise:
                    return noiseBuffer[(int)(phase / TWOPI) % noiseBuffer.Length];
                default:
                    return 0;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float BandLimit(float smp)
        {
            //This is a LPF at 22049hz.
            xv[0] = xv[1];
            xv[1] = smp / 1.000071238f;
            yv[0] = yv[1];
            yv[1] = (xv[0] + xv[1]) + (-0.9998575343f * yv[0]);
            return yv[1];
        }

        float[] noiseBuffer;
        float[] xv = new float[2], yv = new float[2];
    }

}