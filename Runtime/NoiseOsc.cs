using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    [System.Serializable]
    public class NoiseOsc : RackItem<NoiseOsc>
    {
        [SignalRange(0, 1)]
        public Signal amp = new Signal(1);
        public Signal bias = new Signal(0);

        [NonSerialized] public Signal output;

        [NonSerialized] float localAmp = 0;


        public NoiseOsc(float amp, float bias)
        {
            this.amp.localValue = amp;
            this.bias.localValue = bias;
        }

        public override void OnAddToRack(Synthesizer synth)
        {
            output.id = synth.NextOutputID();
        }


        public override void UpdateControl(float[] signals)
        {
            if (control == null) return;
            SyncControlSignal(signals, ref amp, ref control.amp);
            SyncControlSignal(signals, ref bias, ref control.bias);
        }


        public override void Tick(float[] signals)
        {
            localAmp = Lerp(localAmp, amp.GetValue(signals), 0.01f);
            var smp = bias.GetValue(signals) + (Entropy.Next() * 2 - 1) * localAmp;
            output.SetValue(signals, smp);
        }

    }

}