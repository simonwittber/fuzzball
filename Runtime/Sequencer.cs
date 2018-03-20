using System;
using System.Linq;
using System.Runtime.CompilerServices;
using DifferentMethods.Extensions.Collections;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{

    [System.Serializable]
    public class Sequencer : RackItem<Sequencer>
    {

        public SequencerType type;
        public AnimationCurve envelope = AnimationCurve.Constant(0, 1, 1);
        [SignalRange(0, 1)]
        public Signal glide = new Signal(1);
        [SignalRange(0, 1)]
        public Signal amp = new Signal(1);
        public Signal frequencyMultiply = new Signal(1);
        public int beatDivider = 1;
        public Signal transpose = new Signal(0);
        public string code = "";

        [NonSerialized] public Signal gate = new Signal();
        [NonSerialized] public Signal outputEnvelope = new Signal();
        [NonSerialized] public Signal outputTrigger = new Signal();
        [NonSerialized] public Signal output = new Signal();

        [NonSerialized] NoteTrigger[] sequence;
        [NonSerialized] string lastCode;
        [NonSerialized] SequencerType lastType;
        [NonSerialized] int index, beatIndex, beatDuration, lastBeat, position, sample;
        [NonSerialized] float lastGate;

        [NonSerialized] PriorityQueue<NoteTrigger> notes = new PriorityQueue<NoteTrigger>();
        [NonSerialized] NoteTrigger activeNote, nextNote;

        public override string ToString()
        {
            return $"SEQ {type} C:{code} G:{gate} OE:{outputEnvelope} OT:{outputTrigger} O:{output}";
        }

        public Sequencer(SequencerType type, float frequencyMultiply, float transpose, float glide, float amp, string code)
        {
            this.type = type;
            this.frequencyMultiply.localValue = frequencyMultiply;
            this.transpose.localValue = transpose;
            this.glide.localValue = glide;
            this.amp.localValue = amp;
            this.code = code;
            Parse();
        }


        public override void UpdateControl(float[] signals)
        {
            if (control == null) return;
            this.type = control.type;
            SyncControlSignal(signals, ref envelope, ref control.envelope);
            SyncControlSignal(signals, ref glide, ref control.glide);
            SyncControlSignal(signals, ref amp, ref control.amp);
            SyncControlSignal(signals, ref frequencyMultiply, ref control.frequencyMultiply);
            SyncControlSignal(signals, ref transpose, ref control.transpose);
            beatDivider = control.beatDivider;
            this.code = control.code;
        }

        public override void OnAddToRack(Synthesizer synth)
        {
            output.id = synth.NextOutputID();
            outputTrigger.id = synth.NextOutputID();
            outputEnvelope.id = synth.NextOutputID();
            beatIndex = -1;
            Parse();
            ScheduleNoteTriggers(0);
        }

        void Parse()
        {
            var parts = (from i in code.Split(',') select i.Trim()).ToArray();
            sequence = new NoteTrigger[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                var noteTrigger = new NoteTrigger() { duration = 1, volume = 0.9f };
                var pda = (from x in parts[i].Split(':') select x.Trim()).ToArray();
                if (pda.Length >= 1)
                {
                    float hz;
                    if (float.TryParse(pda[0], out hz))
                    {
                        noteTrigger.hz = hz;
                        noteTrigger.noteNumber = -1;
                    }
                    else
                    {
                        noteTrigger.hz = Note.Frequency(pda[0]);
                        noteTrigger.noteNumber = Note.Number(noteTrigger.hz);
                    }
                }
                if (pda.Length >= 2)
                {
                    int duration;
                    if (int.TryParse(pda[1], out duration))
                        noteTrigger.duration = duration;
                }
                if (pda.Length >= 3)
                {
                    float volume;
                    if (float.TryParse(pda[2], out volume))
                        noteTrigger.volume = volume;
                }
                sequence[i] = noteTrigger;
            }
            if (type == SequencerType.Down)
                Reverse();
            lastCode = code;
            lastType = type;
        }


        public override void Tick(float[] signals)
        {
            sample++;
            if (sequence == null || lastCode != code || lastType != type)
            {
                Parse();
                ScheduleNoteTriggers(0);
            }
            position++;
            var gateValue = gate.GetValue(signals);
            outputTrigger.SetValue(signals, -1);
            if (gateValue > 0 && lastGate < 0)
            {
                beatDuration = (sample - lastBeat);
                lastBeat = sample;
                NextBeat(signals);
            }
            var hz = activeNote.hz;

            var N = position * 1f / (beatDuration * activeNote.duration);
            outputEnvelope.SetValue(signals, envelope.Evaluate(N) * activeNote.volume * amp.GetValue(signals));
            var glideStart = (1f - glide.GetValue(signals));
            if (N >= glideStart)
            {
                hz = Mathf.SmoothStep(activeNote.hz, nextNote.hz, Mathf.InverseLerp(glideStart, 1, N));
            }
            lastGate = gateValue;
            output.SetValue(signals, hz);
        }


        void NextBeat(float[] signals)
        {
            beatIndex++;
            if (beatDivider == 0 || beatIndex % beatDivider == 0)
                if (!notes.IsEmpty)
                {
                    var note = notes.Peek();
                    if (note.beat <= beatIndex)
                    {
                        position = 0;
                        outputTrigger.SetValue(signals, 1);
                        activeNote = notes.Pop();
                        var tr = (int)transpose.GetValue(signals);
                        if (tr != 0)
                        {
                            if (activeNote.noteNumber >= 0)
                                activeNote.hz = Note.Frequency(activeNote.noteNumber + tr);
                            else
                                activeNote.hz += tr;
                        }
                        activeNote.hz *= frequencyMultiply.GetValue(signals);
                        if (notes.IsEmpty)
                        {
                            ChangeNoteTriggerPattern();
                            ScheduleNoteTriggers(beatIndex + activeNote.duration);
                        }
                        nextNote = notes.Peek();
                        if (tr != 0 && nextNote.noteNumber >= 0)
                            nextNote.hz = Note.Frequency(nextNote.noteNumber + tr);
                        nextNote.hz *= frequencyMultiply.GetValue(signals);
                    }
                }
        }

        void ScheduleNoteTriggers(int startBeat)
        {
            var b = startBeat;
            notes.Clear();
            for (var i = 0; i < sequence.Length; i++)
            {
                var n = sequence[i];
                n.beat = b;
                notes.Push(n);
                b += n.duration;
            }
        }


        void ChangeNoteTriggerPattern()
        {
            switch (type)
            {
                case SequencerType.Random:
                    Shuffle();
                    break;
                case SequencerType.PingPong:
                    Reverse();
                    break;
            }
        }


        void Reverse()
        {
            System.Array.Reverse(sequence);
        }


        void Shuffle()
        {
            int n = sequence.Length;
            while (n > 1)
            {
                n--;
                int k = (int)(Entropy.Next() * (n + 1));
                var value = sequence[k];
                sequence[k] = sequence[n];
                sequence[n] = value;
            }

        }

    }

}