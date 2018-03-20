using System;
using System.Collections.Generic;
using Fizzle.Designer;

namespace DifferentMethods.FuzzBall
{

    public static class FS
    {
        static Stack<Synthesizer> stack = new Stack<Synthesizer>();

        public static Osc Osc(OscType type, float freq, float amp = 1, float detune = 0, float bias = 0, float duty = 0.5f) => Synth.Add(new Osc(type, freq, amp, detune, bias, duty));
        public static DelayLine DelayLine(float delay = 0.5f, float feedback = 0, float amp = 1) => Synth.Add(new DelayLine(delay, feedback, amp));
        public static Filter Filter(FilterType type = FilterType.Lowpass, float cutoff = 440, float q = 1, float amp = 1) => Synth.Add(new Filter(type, cutoff, q, amp));
        public static Sequencer Sequencer(SequencerType type = SequencerType.Up, float frequencyMultiply = 1f, float transpose = 0, float glide = 0f, float amp = 1f, string code = "") => Synth.Add(new Sequencer(type, frequencyMultiply, transpose, glide, amp, code));
        public static KarplusStrong KarplusStrong() => Synth.Add(new KarplusStrong());
        public static Mixer Mixer(params Signal[] items) => Synth.Add(new Mixer(items));
        public static Transposer Transposer(float multiply, float bias) => Synth.Add(new Transposer(multiply, bias));
        public static Reverb Reverb(float decay = 1, float wet = 1) => Synth.Add(new Reverb(decay, wet));
        public static Percussion Percussion() => Synth.Add(new Percussion());
        public static NoiseOsc NoiseOsc(float amp = 1, float bias = 0) => Synth.Add(new NoiseOsc(amp, bias));
        public static Saturator Saturator(float amt = 0.5f) => Synth.Add(new Saturator(amt));

        static Synthesizer Synth
        {
            get
            {
                return stack.Peek();
            }
        }

        public static Synthesizer Begin(FizzSynth component)
        {
            var synth = new Synthesizer();
            synth.component = component;
            stack.Push(synth);
            return synth;
        }

        public static Synthesizer End()
        {
            var synth = stack.Pop();
            return synth;
        }

    }
}