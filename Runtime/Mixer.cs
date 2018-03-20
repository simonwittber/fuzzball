using System;
using System.Linq;

namespace DifferentMethods.FuzzBall
{

    [System.Serializable]
    public class Mixer : RackItem<Mixer>
    {
        [NonSerialized] public Signal output = new Signal();
        public Signal[] amps;
        [SignalRange(0, 1)]
        public Signal amp = new Signal(1);
        [NonSerialized]
        Signal[] items;

        public override string ToString()
        {
            return string.Join(", ", (from i in items select i.ToString())) + ", O:" + output.ToString();
        }

        public Mixer(Signal[] items)
        {
            this.items = items;
        }

        public override void UpdateControl(float[] signals)
        {
            if (control == null) return;
            if (control.amps.Length != amps.Length)
                control.amps = new Signal[amps.Length];
            for (var i = 0; i < amps.Length; i++)
                SyncControlSignal(signals, ref amps[i], ref control.amps[i]);
            SyncControlSignal(signals, ref amp, ref control.amp);
        }

        public override void OnAddToRack(Synthesizer synth)
        {
            output.id = synth.NextOutputID();
            amps = new Signal[items.Length];
            for (var i = 0; i < items.Length; i++)
                amps[i].localValue = 0.5f;
        }

        public override void Tick(float[] signals)
        {
            var smp = 0f;
            for (var i = 0; i < items.Length; i++)
                smp += items[i].GetValue(signals) * amps[i].GetValue(signals);
            output.SetValue(signals, smp * amp.GetValue(signals));
        }
    }

}