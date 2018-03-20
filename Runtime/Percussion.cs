using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    [System.Serializable]
    public class Percussion : RackItem<Percussion>
    {
        public Osc osc = new Osc(OscType.Sin, 200, 1, 0, 0, 0.5f);
        public AnimationCurve pitchEnvelope = AnimationCurve.EaseInOut(0, 1, 0.5f, 0);
        public AnimationCurve ampEnvelope = AnimationCurve.EaseInOut(0, 1, 0.5f, 0);
        [Space]
        public NoiseOsc noise = new NoiseOsc(1, 0);
        public AnimationCurve noiseEnvelope = AnimationCurve.EaseInOut(0, 0, 0.5f, 1);
        [Space]
        public Filter filter = new Filter(FilterType.Lowpass, 200, 1, 1);

        [NonSerialized] public Signal gate = new Signal();
        [NonSerialized] public Signal output = new Signal();
        [NonSerialized] float lastGate, position;
        InternalMixer mixer;

        public override void OnAddToRack(Synthesizer synth)
        {
            osc.OnAddToRack(synth);
            noise.OnAddToRack(synth);
            filter.OnAddToRack(synth);
            output.id = synth.NextOutputID();
            mixer = new InternalMixer(synth, osc.output, noise.output);
            filter.input.Connect(mixer.output);
            output.Connect(filter.output);
        }

        public override void UpdateControl(float[] signals)
        {
            if (control == null) return;
            osc.ControlledBy(control.osc);
            osc.UpdateControl(signals);
            noise.ControlledBy(control.noise);
            noise.UpdateControl(signals);
            filter.ControlledBy(control.filter);
            filter.UpdateControl(signals);
            SyncControlSignal(signals, ref pitchEnvelope, ref control.pitchEnvelope);
            SyncControlSignal(signals, ref ampEnvelope, ref control.ampEnvelope);
            SyncControlSignal(signals, ref noiseEnvelope, ref control.noiseEnvelope);
        }

        void OnGate(float[] signals)
        {

        }


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
            noise.amp.localValue = noiseEnvelope.Evaluate(position);
            osc.Tick(signals);
            noise.Tick(signals);
            mixer.Tick(signals);
            filter.Tick(signals);
        }

    }

}