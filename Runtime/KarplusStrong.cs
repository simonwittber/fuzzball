using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    [System.Serializable]
    public class KarplusStrong : RackItem<KarplusStrong>
    {
        [SignalRange(0, 22050)]
        public Signal frequency = new Signal(440);
        [SignalRange(0, 11025)]
        public Signal minDecayCycles = new Signal(1);
        [SignalRange(0, 1)]
        public Signal decayProbability = new Signal(1);
        [SignalRange(0, 1)]
        public Signal amp = new Signal(1);

        [NonSerialized] public Signal gate = new Signal();
        [NonSerialized] public Signal output = new Signal();

        [NonSerialized] int period;
        [NonSerialized] float[,] wave = new float[6, 44100];
        [NonSerialized] int activeString = 0;
        [NonSerialized] float lastGate, position;

        public override void OnAddToRack(Synthesizer synth)
        {
            output.id = synth.NextOutputID();
        }

        public override void UpdateControl(float[] signals)
        {
            if (control == null) return;
            SyncControlSignal(signals, ref frequency, ref control.frequency);
            SyncControlSignal(signals, ref minDecayCycles, ref control.minDecayCycles);
            SyncControlSignal(signals, ref decayProbability, ref control.decayProbability);
            SyncControlSignal(signals, ref amp, ref control.amp);
        }

        void OnGate(float[] signals)
        {
            activeString = (activeString + 1) % 6;
            period = Mathf.FloorToInt(SAMPLERATE / (Mathf.Epsilon + frequency.GetValue(signals)));
            for (var i = 0f; i < period; i++)
            {
                wave[activeString, Mathf.FloorToInt(i)] = (Entropy.Next() * 2) - 1;
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Tick(float[] signals)
        {
            var gateValue = gate.GetValue(signals);
            if (gateValue > 0 && lastGate < 0)
            {
                position = 0;
                OnGate(signals);
            }
            else
            {
                position += (1f / Osc.SAMPLERATE);
            }
            lastGate = gateValue;
            var smp = Sample(signals);

            smp = (smp * amp.GetValue(signals));
            output.SetValue(signals, smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float Sample(float[] signals)
        {
            period = Mathf.FloorToInt(SAMPLERATE / (Mathf.Epsilon + frequency.GetValue(signals)));
            if (period <= 0) return 0;
            var duration = period * (1f / SAMPLERATE);
            var sif = (position % duration) * SAMPLERATE;
            var si = (int)sif;
            var ni = (si + 1) % 44100;
            var frac = sif - si;
            var smp = 0f;
            for (var i = 0; i < 6; i++)
            {
                var current = wave[i, si];
                if (Entropy.Next() < decayProbability.GetValue(signals))
                {
                    var prev = Lerp((si > 0 ? wave[i, si - 1] : 0), current, frac);
                    current = Lerp(current, wave[i, ni], frac);
                    current = (prev + current) * 0.5f;
                }
                wave[i, si] = current;
                smp += current;
            }
            return smp;
        }

    }

}