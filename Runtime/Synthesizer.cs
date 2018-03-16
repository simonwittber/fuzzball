using System;
using System.Collections.Generic;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{

    public class Synthesizer
    {
        static int signalCount = 1;
        public List<RackItem> rack = new List<RackItem>();
        int sampleIndex = 0;
        public Signal[] audioOutput;
        List<int> outputSignals = new List<int>();
        List<int> inputSignals = new List<int>();
        float[] signals;

        public Synthesizer()
        {
            audioOutput = new Signal[7];
        }

        public void ConnectAudioOut(int channel, Signal signal)
        {
            audioOutput[channel].id = signal.id;
        }

        public int NextOutputID()
        {
            return signalCount++;
        }

        public T Add<T>(T item) where T : RackItem
        {
            rack.Add(item);
            item.OnAddToRack(this);
            return item;
        }

        public void ReadAudio(float[] data, int channels)
        {
            foreach (var item in rack)
                item.UpdateControl(signals);

            for (var i = 0; i < data.Length; i += channels)
            {
                foreach (var id in outputSignals)
                    signals[id] = 0;
                foreach (var item in rack)
                    item.Tick(signals);
                for (var c = 0; c < channels; c++)
                    data[i + c] = signals[audioOutput[c].id];
            }
        }

        public void Init()
        {
            signals = new float[signalCount];
        }
    }
}