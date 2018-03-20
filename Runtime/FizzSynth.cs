using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;

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
        public float cpuTime;
        System.Diagnostics.Stopwatch stopwatch;

        void OnEnable()
        {
            stopwatch = new System.Diagnostics.Stopwatch();
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
            stopwatch.Reset();
            stopwatch.Start();
            synthesizer.UpdateControl(signals);
            for (var i = 0; i < data.Length; i += channels)
            {
                if (i % 1024 == 0)
                    synthesizer.UpdateControl(signals);
                synthesizer.Tick(signals);
                data[i + 0] = Clamp(synthesizer.outputs[0].GetValue(signals), -1, 1);
                data[i + 1] = Clamp(synthesizer.outputs[1].GetValue(signals), -1, 1);
            }
            stopwatch.Stop();
            cpuTime = Mathf.Lerp(cpuTime, stopwatch.ElapsedMilliseconds, 0.01f);
        }


        protected static float Clamp(float v, float mn, float mx)
        {
            return v < mn ? mn : v > mx ? mx : v;
        }

        [ContextMenu("Export")]
        void Export()
        {
            Init();
            var data = new float[Mathf.FloorToInt((duration * 44100) * 2)];
            Profiler.BeginSample("Fuzz");
            OnAudioFilterRead(data, 2);
            Profiler.EndSample();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filename = "FuzzBall-" + DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss") + ".wav";
            AudioClipExporter.Save(System.IO.Path.Combine(path, filename), data);
        }


    }
}