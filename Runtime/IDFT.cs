using System.Collections.Generic;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    public static class Fourier
    {

        public static float[] IDFT(float[] cos, float[] sin, int len = 0)
        {
            if (len == 0)
                len = (cos.Length - 1) * 2;

            float[] output = new float[len];

            int partials = sin.Length;
            if (partials > len / 2)
                partials = len / 2;

            for (int n = 0; n <= partials; n++)
            {
                for (int i = 0; i < len; i++)
                {
                    output[i] += Mathf.Cos(2 * Mathf.PI * n / len * i) * cos[n];
                    output[i] += Mathf.Sin(2 * Mathf.PI * n / len * i) * sin[n];
                }
            }

            return output;
        }
    }

}