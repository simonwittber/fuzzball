using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    [System.Serializable]
    public class RackItem
    {
        public static int SAMPLERATE = 44100;
        protected const float TWOPI = 6.283185307179586f;

        public virtual void Tick(float[] signals)
        {
        }

        public virtual void OnAddToRack(Synthesizer synth)
        {
        }

        public virtual void UpdateControl(float[] signals)
        {

        }

        protected void SyncControlSignal(float[] signals, ref Signal A, ref Signal C)
        {
            if (A.id == 0)
                A.localValue = C.localValue;
            else
                C.localValue = A.GetValue(signals);
        }

        protected void SyncControlSignal(float[] signals, ref AnimationCurve A, ref AnimationCurve C)
        {
            if (A == null && C != null)
                A = C;
            else
            {
                A = C;
                // A.keys = C.keys;
            }
        }


        public static float Lerp(float a, float b, float t)
        {
            return b * t + a * (1f - t);
        }


        public static float InverseLerp(float a, float b, float value)
        {
            return (value - a) / (b - a);
        }

    }

    public class RackItem<T> : RackItem where T : class
    {
        [NonSerialized] protected T control;

        public T ControlledBy(T other)
        {
            control = other;
            return this as T;
        }

    }

}