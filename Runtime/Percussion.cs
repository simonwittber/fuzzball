using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    [System.Serializable]
    public class Percussion : RackItem<Percussion>
    {
        public AnimationCurve pitchEnvelope = AnimationCurve.EaseInOut(0, 1, 0.5f, 0);
        public AnimationCurve ampEnvelope = AnimationCurve.EaseInOut(0, 1, 0.5f, 0);
        public Osc osc = new Osc(OscType.Sin, 200, 1, 0, 0, 0.5f);
        public Filter filter = new Filter(FilterType.Lowpass, 200, 1, 1);

        [NonSerialized] public Signal gate = new Signal();
        [NonSerialized] public Signal output = new Signal();
        [NonSerialized] float lastGate, position;

        public override void OnAddToRack(Synthesizer synth)
        {
            osc.OnAddToRack(synth);
            filter.OnAddToRack(synth);
            output.id = synth.NextOutputID();
            filter.input.Connect(osc.output);
            output.Connect(filter.output);
        }

        public override void UpdateControl(float[] signals)
        {
            if (control == null) return;
            osc.ControlledBy(control.osc);
            filter.ControlledBy(control.filter);
            osc.UpdateControl(signals);
            filter.UpdateControl(signals);
            SyncControlSignal(signals, ref pitchEnvelope, ref control.pitchEnvelope);
            SyncControlSignal(signals, ref ampEnvelope, ref control.ampEnvelope);
        }

        void OnGate(float[] signals)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Tick(float[] signals)
        {
            var gateValue = gate.GetValue(signals);
            if (gateValue > 0 && lastGate < 0)
            {
                position = 0;
                output.SetValue(signals, +1);
                OnGate(signals);
            }
            else
            {
                output.SetValue(signals, -1);
                position += (1f / Osc.SAMPLERATE);
            }
            lastGate = gateValue;

            osc.freq.localValue = pitchEnvelope.Evaluate(position);
            osc.amp.localValue = ampEnvelope.Evaluate(position);
            filter.Tick(signals);
            osc.Tick(signals);
        }

    }

}