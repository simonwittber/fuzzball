using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    public static class Entropy
    {
        [ThreadStatic] static uint[] state = new uint[] { 13, 29, 61, 1337 };


        public static float Next()
        {
            uint s, t;
            s = t = state[3];
            t ^= t << 11;
            t ^= t >> 8;
            state[3] = state[2]; state[2] = state[1]; state[1] = s = state[0];
            t ^= s;
            t ^= s >> 19;
            state[0] = t;
            return 1f * t / uint.MaxValue;
        }

    }

}