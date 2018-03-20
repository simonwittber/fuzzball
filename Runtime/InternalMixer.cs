using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DifferentMethods.FuzzBall
{
    [System.Serializable]
    public class InternalMixer
    {
        [NonSerialized] public Signal output = new Signal();
        [NonSerialized] Signal[] items;

        public InternalMixer(Synthesizer synth, params Signal[] items)
        {
            this.output.id = synth.NextOutputID();
            this.items = items;
        }


        public void Tick(float[] signals)
        {
            var smp = 0f;
            for (var i = 0; i < items.Length; i++)
                smp += items[i].GetValue(signals);
            output.SetValue(signals, smp);
        }
    }

}