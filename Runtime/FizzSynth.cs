using System;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(AudioSource))]
    public class FizzSynth : MonoBehaviour
    {
        public Synthesizer synthesizer;

        [NonSerialized] int signalCount = 1;
        public float[] signals;

        void OnEnable()
        {
            signalCount = 1;
            Init();
        }

        public void Init()
        {
            synthesizer = ConstructRack();
            signals = new float[signalCount];
        }

        public virtual Synthesizer ConstructRack()
        {
            throw new System.NotImplementedException();
        }

        internal int NextOutputID()
        {
            return signalCount++;
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            synthesizer.UpdateControl(signals);
            for (var i = 0; i < data.Length; i += channels)
            {
                synthesizer.Tick(signals);
                data[i + 0] = synthesizer.outputs[0].GetValue(signals);
                data[i + 1] = synthesizer.outputs[1].GetValue(signals);
            }
        }


    }
}