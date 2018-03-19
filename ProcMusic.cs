using System.Collections;
using System.Collections.Generic;
using DifferentMethods.FuzzBall;
using static DifferentMethods.FuzzBall.FS;
using UnityEngine;

public class ProcMusic : FizzSynth
{
    public Osc beat;
    public Reverb reverb;

    Synthesizer Bleeper()
    {
        var synthesizer = Begin(this);
        var b = Osc(OscType.Sin, 2).ControlledBy(beat);
        var tr = Sequencer(code: "1,-1,0,3,5,-4", type: SequencerType.Random);
        tr.beatDivider = 16;
        tr.gate.Connect(b.output);
        var s = Sequencer(code: "A3,C4,E4");
        s.gate.Connect(b.output);
        s.transpose.Connect(tr.output);
        var o1 = Osc(OscType.Saw, 440);

        var o2 = Osc(OscType.Saw, 0, detune: 1);

        var tr2 = Transposer(1f, 3);
        tr2.input.Connect(s.output);

        o1.freq.Connect(s.output);
        o2.freq.Connect(tr2.output);

        var m = Mixer(o1.output, o2.output);

        o1.amp.Connect(s.outputEnvelope);
        o2.amp.Connect(s.outputEnvelope);


        synthesizer.outputs[0].Connect(m.output);
        synthesizer.outputs[1].Connect(m.output);

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
