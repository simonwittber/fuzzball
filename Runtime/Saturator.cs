using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    [System.Serializable]
    public class Saturator : RackItem<Saturator>
    {
        public Signal input;
        public Signal output;


        public Signal amt = new Signal(0.5f);

        public Saturator(float amt)
        {
            this.amt.localValue = amt;
        }

        public override void OnAddToRack(Synthesizer synth)
        {
            output.id = synth.NextOutputID();
        }

        public override void UpdateControl(float[] signals)
        {
            if (control == null) return;
            SyncControlSignal(signals, ref amt, ref control.amt);
        }

        public override void Tick(float[] signals)
        {
            var x = input.GetValue(signals);
            var a = amt.GetValue(signals);
            var fx = x;
            if (x < a)
                fx = x;
            else if (x > a)
                fx = a + (x - a) / (1 + Mathf.Pow(((x - a) / (1 - a)), 2));
            else if (x > 1)
                fx = (a + 1) / 2;
            output.SetValue(signals, fx);
        }

    }

}