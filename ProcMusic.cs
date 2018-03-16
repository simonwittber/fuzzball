using System.Collections;
using System.Collections.Generic;
using DifferentMethods.FuzzBall;
using static DifferentMethods.FuzzBall.FS;
using UnityEngine;

public class ProcMusic : FizzSynth
{
    public Osc beat;

    public Mixer mixer;
    public Sequencer sequencer;
    public Osc osc1, osc2;

    public override Synthesizer ConstructRack()
    {
        synthesizer = Begin();
        var b = Osc(OscType.Sin, 2).ControlledBy(beat);
        var tr = Sequencer(code: "1,-1,0,3,5,-4", type: SequencerType.Random);
        tr.beatDivider = 16;
        tr.gate.Connect(b.output);
        var s = Sequencer(code: "A3,C4,E4").ControlledBy(sequencer);
        s.gate.Connect(b.output);
        s.transpose.Connect(tr.output);

        var o1 = Osc(OscType.Sin, 0).ControlledBy(osc1);
        var o2 = Osc(OscType.Sin, 0, detune: 3).ControlledBy(osc2);
        o1.freq.Connect(s.output);
        o2.freq.Connect(s.output);

        var m = Mixer(o1.output, o2.output).ControlledBy(mixer);
        m.amp.Connect(s.outputEnvelope);
        // o1.amp.Connect(s.outputEnvelope);
        // o2.amp.Connect(s.outputEnvelope);

        synthesizer.audioOutput[0].Connect(m.output);
        synthesizer.audioOutput[1].Connect(m.output);
        return End();
    }

}
