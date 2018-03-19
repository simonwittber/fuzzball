using System;

namespace DifferentMethods.FuzzBall
{
    [System.Serializable]
    public class Transposer : RackItem<Transposer>
    {
        [NonSerialized] public Signal input = new Signal();
        public Signal multiply = new Signal(1);
        public Signal bias = new Signal(0);
        [NonSerialized] public Signal output = new Signal();

        [NonSerialized] float[] buffer = new float[44100 * 10];

        [System.NonSerialized] float time = 0;
        [System.NonSerialized] int index = 0;

        public Transposer(float multiply, float bias)
        {
            this.multiply.localValue = multiply;
            this.bias.localValue = bias;
        }

        public override void OnAddToRack(Synthesizer synth)
        {
            output.id = synth.NextOutputID();
        }

        public override void Tick(float[] signals)
        {
            var smp = (input.GetValue(signals) * multiply.GetValue(signals)) + bias.GetValue(signals);
            output.SetValue(signals, smp);
        }

    }

}