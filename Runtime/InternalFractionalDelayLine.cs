using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    public class InternalFractionalDelayLine
    {

        const int MAX_BUF_SIZE = 64000;

        float[] d_buffer = new float[MAX_BUF_SIZE];

        struct fract_delay
        {
            public float d_mix;       /*delay blend parameter*/
            public int d_samples;    /*delay duration in samples*/
            public float d_fb;        /*feedback volume*/
            public float d_fw;        /*delay tap mix volume*/
            public float n_fract;     /*fractional part of the delay*/
            public int rdPtr;      /*delay read pointer*/
            public int wrtPtr;     /*delay write pointer*/
        };

        fract_delay del;


        public void Delay_Init(float delay_samples, float dfb, float dfw, float dmix)
        {
            Delay_set_delay(delay_samples);
            Delay_set_fb(dfb);
            Delay_set_fw(dfw);
            Delay_set_mix(dmix);
            del.wrtPtr = MAX_BUF_SIZE - 1;
        }


        public void Delay_set_fb(float val)
        {
            del.d_fb = val;
        }

        public void Delay_set_fw(float val)
        {
            del.d_fw = val;
        }

        public void Delay_set_mix(float val)
        {
            del.d_mix = val;
        }

        public void Delay_set_delay(float n_delay)
        {
            /*Get the integer part of the delay*/
            del.d_samples = Mathf.FloorToInt(n_delay);

            /*gets the fractional part of the delay*/
            del.n_fract = (n_delay - del.d_samples);
        }

        public float Delay_get_fb()
        {
            return del.d_fb;
        }

        public float Delay_get_fw()
        {
            return del.d_fw;
        }

        public float Delay_get_mix()
        {
            return del.d_mix;
        }

        /*
        This is the main delay task,
        */
        public float Delay_task(float xin)
        {
            float yout;
            int y0;
            int y1;
            float x1;
            float x_est;

            /*Calculates current read pointer position*/
            del.rdPtr = del.wrtPtr - (short)del.d_samples;

            /*Wraps read pointer*/
            if (del.rdPtr < 0)
            {
                del.rdPtr += MAX_BUF_SIZE - 1;
            }

            /*Linear interpolation to estimate the delay + the fractional part*/
            y0 = del.rdPtr - 1;
            y1 = del.rdPtr;

            if (y0 < 0)
            {
                y0 += MAX_BUF_SIZE - 1;
            }

            x_est = (d_buffer[y0] - d_buffer[y1]) * del.n_fract + d_buffer[y1];

            /*Calculate next value to store in buffer*/
            x1 = xin + x_est * del.d_fb;

            /*Store value in buffer*/
            d_buffer[del.wrtPtr] = x1;

            /*Output value calculation*/
            yout = x1 * del.d_mix + x_est * del.d_fw;

            /*Increment delat write pointer*/
            del.wrtPtr++;

            /*Wraps delay write pointer*/
            if (del.wrtPtr > MAX_BUF_SIZE - 1)
            {
                del.wrtPtr = 0;
            }
            return yout;
        }

    }
}