using System.Runtime.CompilerServices;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{

    [System.Serializable]
    public class Reverb : RackItem<Reverb>
    {
        public Signal[] inputs = new Signal[2];
        public Signal[] outputs = new Signal[2];
        public Signal decay = new Signal(1);
        public Signal wet = new Signal(1);

        float allpassCoeff = 0.7f;
        float comb1Coeff;
        float comb2Coeff;
        int[] reverbDelay = { 341, 613, 1557, 2137 };

        InternalDelayLine allpass1;
        InternalDelayLine allpass2;
        InternalDelayLine comb1;
        InternalDelayLine comb2;

        public Reverb(float decay, float wet)
        {
            this.decay.localValue = decay;
            this.wet.localValue = wet;
        }

        public override void OnAddToRack(Synthesizer synth)
        {
            outputs[0].id = synth.NextOutputID();
            outputs[1].id = synth.NextOutputID();

            if (SAMPLERATE != 44100)
            {
                var samplesPerMillisecond = 88;
                var scaler = SAMPLERATE / 44100.0f;
                samplesPerMillisecond = Mathf.FloorToInt(scaler * samplesPerMillisecond);
                for (var i = 0; i < reverbDelay.Length; i++)
                {
                    var delay = Mathf.FloorToInt(scaler * reverbDelay[i]);
                    if ((delay & 1) == 0)
                    {
                        delay++;
                    }
                    while (!IsPrime(delay))
                        delay += 2;
                    reverbDelay[i] = delay;
                }
            }

            allpass1 = new InternalDelayLine(reverbDelay[0]);
            allpass2 = new InternalDelayLine(reverbDelay[1]);
            comb1 = new InternalDelayLine(reverbDelay[2]);
            comb2 = new InternalDelayLine(reverbDelay[3]);
        }

        bool IsPrime(int N)
        {
            if ((N & 1) == 1)
            {
                var upto = (int)Mathf.Sqrt(N);
                for (var i = 3; i <= upto; i += 2)
                {
                    if (N % i == 0)
                        return false;
                }
                return true;
            }
            else
            {
                return (N == 2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateControl(float[] signals)
        {
            if (control == null) return;
            SyncControlSignal(signals, ref decay, ref control.decay);
            SyncControlSignal(signals, ref wet, ref control.wet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Tick(float[] signals)
        {
            var fadeMix = wet.GetValue(signals);
            var combScale = -3.0f / (decay.GetValue(signals) * SAMPLERATE);
            comb1Coeff = Mathf.Pow(10.0f, combScale * comb1.Length);
            comb2Coeff = Mathf.Pow(10.0f, combScale * comb2.Length);

            var smpL = inputs[0].GetValue(signals);
            var smpR = inputs[1].GetValue(signals);
            var input = 0.5f * (smpL + smpR);
            var temp0 = allpassCoeff * allpass1.Last;
            temp0 += input;
            temp0 = allpass1.Tick(temp0) - allpassCoeff * temp0;
            var temp1 = allpassCoeff * allpass2.Last;
            temp1 += temp0;
            temp1 = allpass2.Tick(temp1) - allpassCoeff * temp1;
            var out1 = comb1.Tick(temp1 + comb1Coeff * comb1.Last);
            var out2 = comb2.Tick(temp1 + comb2Coeff * comb2.Last);
            out1 = fadeMix * out1 + (1.0f - fadeMix) * smpL;
            out2 = fadeMix * out2 + (1.0f - fadeMix) * smpR;
            smpL = out1;
            smpR = out2;
            outputs[0].SetValue(signals, smpL);
            outputs[1].SetValue(signals, smpR);
        }
    }

}