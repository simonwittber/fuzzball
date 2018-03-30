using System.Collections;
using System.Collections.Generic;
using DifferentMethods.FuzzBall;
using static DifferentMethods.FuzzBall.FS;
using UnityEngine;
using System;

public class ProcMusic : FizzSynth
{
    public Osc beat, noise;
    public Percussion percussion;
    public Reverb reverb;
    public Sequencer sequencer;
    public Saturator saturator;
    public KarplusStrong karplusStrong;


    Synthesizer Bleeper()
    {
        var synthesizer = Begin(this);
        var b = Osc(OscType.Sin, 2).ControlledBy(beat);
        var tr = Sequencer(code: "1,-1,0,3,5,-4", type: SequencerType.Random);
        tr.beatDivider = 16;
        tr.gate.Connect(b.output);
        var s = Sequencer(code: "A3,C3,G#3:4,A2:2", type: SequencerType.Random, glide: 0.05f).ControlledBy(sequencer);
        s.gate.Connect(b.output);
        s.transpose.Connect(tr.output);

        var o1 = KarplusStrong().ControlledBy(karplusStrong);
        o1.damping.localValue = 0.25f;


        o1.frequency.Connect(s.output);
        o1.gate.Connect(s.outputTrigger);
        o1.amp.Connect(s.outputEnvelope);
        var sa = Saturator();
        sa.input.Connect(o1.output);


        synthesizer.outputs[0].Connect(sa.output);
        synthesizer.outputs[1].Connect(sa.output);

        return End();
    }

    public override Synthesizer ConstructRack()
    {
        var synthesizer = Begin(this);
        var b = Osc(OscType.Sin, 2).ControlledBy(beat);
        var perc = Percussion().ControlledBy(percussion);
        perc.gate.Connect(b.output);
        var vb = Reverb(1, 0.5f).ControlledBy(reverb);
        vb.inputs[0].Connect(perc.output);
        var m = Mixer(vb.outputs[0], vb.outputs[1]);
        synthesizer.outputs[0].Connect(m.output);
        synthesizer.outputs[1].Connect(m.output);
        return End();
    }

}
