using UnityEngine;

namespace Nothke.Utils
{
    [System.Serializable]
    public struct ADSREnvelope
    {
        public float attack, decay, sustain, release;

        public float attackEase;
        public float decayEase;
        public float releaseEase;

        public bool interrupt;

        float time;
        bool lastPressed;
        float lastOnValue;

        public float Time => time;

        public float EvaulateIn(float time)
        {
            if (time < attack)
                return 1 - Ease(1 - time / attack, attackEase);

            else if (time < attack + decay)
                return Mathf.Lerp(sustain, 1, Ease(1 - ((time - attack) / decay), decayEase));

            else
                return sustain;
        }

        public float EvaluateOut(float time, float from = 0)
        {
            float _from = from == 0 ? sustain : from;

            if (time < 0)
                return _from;

            else if (time < release)
                return Ease(1 - time / release, releaseEase) * _from;

            else
                return 0;
        }

        public float Update(bool value, float dt)
        {

            // If interrup is not set, this makes the value "sticky",
            // so it keeps being on until the end of decay
            if (lastPressed && !interrupt && time < attack + decay)
            {
                value = true;
            }

            // Reset time on key change
            if (value != lastPressed)
                time = 0;

            time += dt;

            float f;
            if (value)
            {
                f = EvaulateIn(time);
                lastOnValue = f;
            }
            else
            {
                f = EvaluateOut(time, lastOnValue);
            }

            lastPressed = value;

            return f;
        }

        static float Ease(float p_x, float p_c)
        {
            if (p_c == 0)
            {
                return p_x;
            }
            else if (p_c < 0)
            {
                return 1.0f - Mathf.Pow(1.0f - p_x, -p_c + 1);
            }
            else
            {
                return Mathf.Pow(p_x, p_c + 1);
            }
        }

        /*
        // Overshoots for |p_c| > 3
        public static float Ease(float p_x, float p_c)
        {
            if (p_x < 0)
                return 0;
            else if (p_x > 1.0f)
                return 1.0f;

            return ((p_c * (1.0f - p_x) + 2.0f) * (1.0f - p_x) + 1.0f) * p_x * p_x;
        }*/
    }
}