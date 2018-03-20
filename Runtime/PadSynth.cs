using System;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    public class PadSynth : RackItem<PadSynth>
    {
        int N;
        int samplerate;
        int number_harmonics;
        float[] A, freq_amp;

        public PadSynth(int number_harmonics, int length = 880, int sampleRate = 44100)
        {
            this.N = length;
            this.samplerate = sampleRate;
            this.number_harmonics = number_harmonics;
            A = new float[number_harmonics];
            for (int i = 0; i < number_harmonics; i++) A[i] = 0.0f;
            A[1] = 1.0f; //default, the first harmonic has the amplitude 1.0

            freq_amp = new float[N / 2];
        }

        public void SetHarmonic(int n, float value)
        {
            if ((n < 1) || (n >= number_harmonics)) return;
            A[n] = value;
        }

        public float GetHarmonic(int n)
        {
            if ((n < 1) || (n >= number_harmonics)) return 0.0f;
            return A[n];
        }

        public float[] CreateWave(float frequency = 440f, float bandwidth = 25f, float bandwidthScale = 1f)
        {
            int i, nh;

            for (i = 0; i < N / 2; i++) freq_amp[i] = 0.0f; //default, all the frequency amplitudes are zero

            //for each harmonic
            for (nh = 1; nh < number_harmonics; nh++)
            {
                float bw_Hz; //bandwidth of the current harmonic measured in Hz
                float bwi;
                float fi;
                float rF = frequency * RelF(nh);

                bw_Hz = (Mathf.Pow(2.0f, bandwidth / 1200.0f) - 1.0f) * frequency * Mathf.Pow(RelF(nh), bandwidthScale);

                bwi = bw_Hz / (2.0f * samplerate);
                fi = rF / samplerate;
                //here you can optimize, by avoiding to compute the profile for the full frequency (usually it's zero or very close to zero)
                for (i = 0; i < N / 2; i++)
                {
                    float hprofile;
                    hprofile = Profile((i / (float)N) - fi, bwi);
                    freq_amp[i] += hprofile * A[nh];
                }
            }

            float[] freq_real = new float[N / 2];
            float[] freq_imaginary = new float[N / 2];

            //Convert the freq_amp array to complex array (real/imaginary) by making the phases random
            for (i = 0; i < N / 2; i++)
            {
                float phase = UnityEngine.Random.value * 2.0f * 3.14159265358979f;
                freq_real[i] = freq_amp[i] * Mathf.Cos(phase);
                freq_imaginary[i] = freq_amp[i] * Mathf.Sin(phase);
            }
            // var waveOutput = Fourier.IDFT(freq_imaginary, freq_real);
            freq_real = null;
            freq_imaginary = null;
            //normalize the output
            // float max = 0.0f;
            // for (i = 0; i < N; i++) if (Mathf.Abs(waveOutput[i]) > max) max = Mathf.Abs(waveOutput[i]);
            // if (max < 1e-5) max = (float)1e-5;
            // for (i = 0; i < N; i++) waveOutput[i] /= max * 1.4142f;
            return null;
            // return waveOutput;
        }


        float RelF(int N)
        {
            return N;
        }

        float Profile(float fi, float bwi)
        {
            float x = fi / bwi;
            x *= x;
            if (x > 14.71280603f) return 0.0f; //this avoids computing the e^(-x^2) where it's results are very close to zero
            return Mathf.Exp(-x) / bwi;
        }

    }

}