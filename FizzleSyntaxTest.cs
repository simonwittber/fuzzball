using System.Collections;
using System.Collections.Generic;
using DifferentMethods.FuzzBall;
using static DifferentMethods.FuzzBall.FS;
using UnityEngine;

public class FizzleSyntaxTest : FizzSynth
{
    public Sequencer sequencer;
    public Osc osc, sound;
    public DelayLine delay;
    public Filter filter;
    public KarplusStrong karp;
    public Mixer mixer;

    public override Synthesizer ConstructRack()
    {
        synthesizer = Begin();
        var o3 = Osc(OscType.Saw, 55);
        var o1 = Osc(OscType.Sin, 0.5f, amp: 0.5f, bias: 0.5f).ControlledBy(sound);
        var o2 = Osc(OscType.WaveShape, 220).ControlledBy(osc).ChainOutput(o3.output, 0.5f);

        o2.amp.Connect(o1.output);
        var de = DelayLine().ControlledBy(delay);
        de.input.Connect(o2.output);
        var f = Filter().ControlledBy(filter);
        f.input.Connect(de.output);
        var s = Sequencer().ControlledBy(sequencer);
        o1.freq.Connect(s.output);
        var beat = Osc(OscType.Sin, 1);
        s.gate.Connect(beat.output);
        var k = KarplusStrong().ControlledBy(karp);
        k.gate.Connect(s.outputTrigger);
        k.frequency.Connect(s.output);

        var m = Mixer(k.output, o2.output, o3.output).ControlledBy(mixer);

        synthesizer.audioOutput[0].Connect(m.output);
        synthesizer.audioOutput[1].Connect(f.output);
        return End();
    }

}
