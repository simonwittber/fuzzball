using System.Collections;
using System.Collections.Generic;
using DifferentMethods.FuzzBall;
using static DifferentMethods.FuzzBall.FS;
using UnityEngine;

public class ProcMusic : FizzSynth
{
    public Osc beat;
    public Reverb reverb;
    public Saturator saturator;

    Synthesizer Bleeper()
    {
        var synthesizer = Begin(this);
        var b = Osc(OscType.Sin, 2).ControlledBy(beat);
        var tr = Sequencer(code: "1,-1,0,3,5,-4", type: SequencerType.Random);
        tr.beatDivider = 16;
        tr.gate.Connect(b.output);
        var s = Sequencer(code: "A3,C3,G#3:4,A2:2", type: SequencerType.Random, glide: 0.05f);
        s.gate.Connect(b.output);
        s.transpose.Connect(tr.output);

        var o1 = KarplusStrong();
        o1.sustain.localValue = 0.25f;


        o1.frequency.Connect(s.output);
        o1.gate.Connect(s.outputTrigger);
        o1.amp.Connect(s.outputEnvelope);

        synthesizer.outputs[0].Connect(o1.output);
        synthesizer.outputs[1].Connect(o1.output);

        return End();
    }

    public override Synthesizer ConstructRack()
    {
        var synthesizer = Begin(this);
        var bleeper = Bleeper();
        synthesizer.Add(bleeper);
        var bleeper2 = Bleeper();
        synthesizer.Add(bleeper2);

        var rv = FS.Reverb(1).ControlledBy(reverb);
        rv.inputs[0].Connect(bleeper.outputs[0]);
        rv.inputs[1].Connect(bleeper.outputs[1]);

        synthesizer.outputs[0].Connect(rv.outputs[0]);
        synthesizer.outputs[1].Connect(rv.outputs[1]);
        return End();
    }

}
