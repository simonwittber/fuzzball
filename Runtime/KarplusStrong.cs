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

        [NonSerialized] int period, loopCount, sampleIndex;
        [NonSerialized] float[,] wave = new float[6, 44100];
        [NonSerialized] int activeString = 0;
        [NonSerialized] float lastGate, position;

        public override void OnAddToRack(Synthesizer synth)
        {
            output.id = synth.NextOutputID();
        }

        public override void UpdateControl(float[] signals)
        {
            SyncControlSignal(signals, ref frequency, ref control.frequency);
            SyncControlSignal(signals, ref minDecayCycles, ref control.minDecayCycles);
            SyncControlSignal(signals, ref decayProbability, ref control.decayProbability);
            SyncControlSignal(signals, ref amp, ref control.amp);
        }

        void OnGate()
        {
            activeString = (activeString + 1) % 6;
            loopCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Tick(float[] signals)
        {
            var gateValue = gate.GetValue(signals);
            if (gateValue > 0 && lastGate < 0)
            {
                position = 0;
                sampleIndex = 0;
                OnGate();
            }
            else
            {
                position += (1f / Osc.SAMPLERATE);
                sampleIndex++;
            }
            lastGate = gateValue;
            var smp = Sample(signals);

            smp = (smp * amp.GetValue(signals));
            output.SetValue(signals, smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float Sample(float[] signals)
        {
            var freq = frequency.GetValue(signals);
            if (Mathf.Approximately(freq, 0)) return 0;

            var periodF = (SAMPLERATE / (Mathf.Epsilon + frequency.GetValue(signals)));
            period = (int)periodF;
            if (period <= 0) return 0;
            var frac = periodF - period;

            var si = sampleIndex % period;
            if (si == 0) loopCount++;

            var smp = 0f;
            if (sampleIndex < period)
            {
                smp = (Entropy.Next() * 2) - 1;
                wave[activeString, si] += smp;
            }

            var doFilter = sampleIndex > period;
            for (var i = 0; i < 6; i++)
            {
                if (!(doFilter && activeString == i))
                {
                    var prev = (si > 0 ? wave[i, si - 1] : 0);
                    var mustFilter = sampleIndex < (period * minDecayCycles.GetValue(signals));
                    if (mustFilter)
                        wave[i, si] = (prev + wave[i, si]) * 0.5f;
                    else if (Entropy.Next() <= decayProbability.GetValue(signals))
                        wave[i, si] = (prev + wave[i, si]) * 0.5f;
                    smp += Mathf.Lerp(wave[i, si], wave[i, (si + 1) % (period + 1)], frac);
                }
            }
            return smp;
        }

    }

}