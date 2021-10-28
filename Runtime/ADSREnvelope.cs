///
/// ADSREnvelope.cs by Nothke
///
/// Attack-Decay-Sustain-Release envelope helper struct. 
/// Also comes with an optional, but recommended, custom property drawer
/// (see Editor/ADSREnvelopeDrawer.cs)
///
/// Important notes:
/// * When using it as a field, initialize one with .Default(),
///   so that values are properly initialized, like this:
///   `public ADSREnvelope envelope = ADSREnvelope.Default();`
/// * To get a value, use envelope.Update(signalBool, deltaTime);
/// * Use interrupt if you want the value to be interrupted immediately 
///   when signal is cut (see tooltip)
///
///
/// ============================================================================
///
/// MIT License
///
/// Copyright(c) 2021 Ivan Notaroš
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// 
/// ============================================================================
///

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

        [Tooltip("If disabled, no matter how short the signal is, it will be played until at least the end of decay time. " +
            "\n\nIf enabled, the end of signal will \"interrupt\" the attack or decay and immediatelly skip to release.")]
        public bool interrupt;

        float time;
        bool lastPressed;
        float lastOnValue;

        public float Time => time;

        public static ADSREnvelope Default()
        {
            return new ADSREnvelope()
            {
                attack = 1,
                decay = 1,
                sustain = 0.5f,
                release = 1
            };
        }

        public float EvaluateIn(float time)
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
            // If interrupt is not set, this makes the value "sticky",
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
                f = EvaluateIn(time);
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
    }
}
