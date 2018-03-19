using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    [System.Serializable]
    public class Synthesizer : RackItem<Synthesizer>
    {

        public List<RackItem> rack = new List<RackItem>();
        public Signal[] inputs;
        public Signal[] outputs;

        internal FizzSynth component;

        public Synthesizer()
        {
            inputs = new Signal[2];
            outputs = new Signal[2];
        }

        public void ConnectAudioOut(int channel, Signal signal)
        {
            outputs[channel].id = signal.id;
        }

        public int NextOutputID()
        {
            return component.NextOutputID();
        }

        public T Add<T>(T item) where T : RackItem
        {
            rack.Add(item);
            item.OnAddToRack(this);
            return item;
        }

        public override string ToString()
        {
            var racks = string.Join("\n", (from i in rack select i.ToString()));
            var outs = string.Join("\n", (from i in outputs select i.ToString()));
            return $"{racks}\n{outs}";
        }

        public override void Tick(float[] signals)
        {
            foreach (var item in rack)
                item.Tick(signals);
        }

        public override void OnAddToRack(Synthesizer synth)
        {

        }

        public override void UpdateControl(float[] signals)
        {
            foreach (var item in rack)
                item.UpdateControl(signals);
        }
    }
}