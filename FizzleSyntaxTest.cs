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
        synthesizer = Begin(this);

        return End();
    }

}
