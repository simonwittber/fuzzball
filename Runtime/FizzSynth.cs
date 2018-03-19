using System;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(AudioSource))]
    public class FizzSynth : MonoBehaviour
    {
        public float duration = 180;

        protected Synthesizer synthesizer;
        [NonSerialized] int signalCount = 1;
        public float[] signals;

        void OnEnable()
        {
            Init();
        }

        public void Init()
        {
            signalCount = 1;
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
                if (i % 1024 == 0)
                    synthesizer.UpdateControl(signals);
                synthesizer.Tick(signals);
                data[i + 0] = synthesizer.outputs[0].GetValue(signals);
                data[i + 1] = synthesizer.outputs[1].GetValue(signals);
            }
        }

        [ContextMenu("Export")]
        void Export()
        {
            Init();
            var data = new float[Mathf.FloorToInt((duration * 44100) * 2)];
            OnAudioFilterRead(data, 2);
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filename = "FuzzBall-" + DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss") + ".wav";
            AudioClipExporter.Save(System.IO.Path.Combine(path, filename), data);
        }


    }
}