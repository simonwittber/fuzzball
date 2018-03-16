using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{

    [System.Serializable]
    public class Filter : RackItem<Filter>
    {
        public FilterType type;
        public Signal input = new Signal();
        public AnimationCurve waveshaper = AnimationCurve.Linear(0, -1, 1, 1);
        [SignalRange(0, 22050)]
        public Signal cutoff = new Signal(440);
        [SignalRange(0, 22)]
        public Signal q = new Signal(1);
        [SignalRange(0, 1)]
        public Signal amp = new Signal(1);
        [NonSerialized] public Signal output = new Signal();

        [NonSerialized] BQFilter bqFilter = new BQFilter();
        [NonSerialized] float lastC, lastQ;
        [NonSerialized] FilterType lastType;

        public Filter(FilterType type, float cutoff, float q, float amp)
        {
            this.type = type;
            this.cutoff.localValue = cutoff;
            this.q.localValue = q;
            this.amp.localValue = amp;
        }

        public override string ToString()
        {
            return $"Filter: {type} Cutoff:{cutoff} Q:{q} Amp:{amp}";
        }

        public override void OnAddToRack(Synthesizer synth)
        {
            output.id = synth.NextOutputID();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateControl(float[] signals)
        {
            if (control == null) return;
            type = control.type;
            if (this.waveshaper == null && control.waveshaper != null)
                this.waveshaper = control.waveshaper;
            else
                this.waveshaper.keys = control.waveshaper.keys;
            SyncControlSignal(signals, ref cutoff, ref control.cutoff);
            SyncControlSignal(signals, ref q, ref control.q);
            SyncControlSignal(signals, ref amp, ref control.amp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Tick(float[] signals)
        {
            var smp = Sample(signals, input.GetValue(signals));
            smp = (smp * amp.GetValue(signals));
            output.SetValue(signals, smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float Sample(float[] jacks, float smp)
        {
            var c = cutoff.GetValue(jacks);
            var r = q.GetValue(jacks);
            if (c != lastC || r != lastQ || type != lastType)
            {
                lastC = c; lastQ = r; lastType = type;
                switch (type)
                {
                    case FilterType.Lowpass:
                        bqFilter.SetLowPass(c, r);
                        break;
                    case FilterType.Highpass:
                        bqFilter.SetHighPass(c, r);
                        break;
                    case FilterType.Bandpass:
                        bqFilter.SetBandPass(c, r);
                        break;
                    case FilterType.Bandstop:
                        bqFilter.SetBandStop(c, r);
                        break;
                    case FilterType.Allpass:
                        bqFilter.SetAllPass(c, r);
                        break;
                }
            }

            switch (type)
            {
                case FilterType.PassThru: return smp;
                case FilterType.Lowpass:
                case FilterType.Highpass:
                case FilterType.Bandpass:
                case FilterType.Bandstop:
                case FilterType.Allpass:
                    return bqFilter.Update(smp);
                case FilterType.Waveshaper:
                    return waveshaper.Evaluate(smp);
            }
            return 0f;
        }

    }

}