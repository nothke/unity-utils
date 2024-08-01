///
/// ADSREnvelope.cs by Nothke
///
/// Attack-Decay-Sustain-Release envelope helper struct. 
/// Also comes with a custom property drawer
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

#if UNITY_EDITOR
using UnityEditor;
#endif

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

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ADSREnvelope))]
    public class ADSREnvelopeDrawer : PropertyDrawer
    {
        const int propCount = 7;
        bool fold;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;
            return !fold ? h : propCount * h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;
            float fullWidth = position.width;

            EditorGUI.BeginProperty(position, label, property);

            float startX = position.x;

            // Draw label foldout
            Rect foldRect = position;
            foldRect.height = h;
            fold = EditorGUI.Foldout(foldRect, fold, label, true);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(" "));

            var attackProp = property.FindPropertyRelative("attack");
            var decayProp = property.FindPropertyRelative("decay");
            var sustainProp = property.FindPropertyRelative("sustain");
            var releaseProp = property.FindPropertyRelative("release");

            var attackEaseProp = property.FindPropertyRelative("attackEase");
            var decayEaseProp = property.FindPropertyRelative("decayEase");
            var releaseEaseProp = property.FindPropertyRelative("releaseEase");

            var interruptProp = property.FindPropertyRelative("interrupt");

            ADSREnvelope adsr = new ADSREnvelope
            {
                attack = attackProp.floatValue,
                decay = decayProp.floatValue,
                sustain = sustainProp.floatValue,
                release = releaseProp.floatValue,

                attackEase = attackEaseProp.floatValue,
                decayEase = decayEaseProp.floatValue,
                releaseEase = releaseEaseProp.floatValue,

                interrupt = interruptProp.boolValue
            };

            Rect curveRect = position;

            if (!fold)
                curveRect.height = h;
            else
            {
                curveRect.x = startX;
                curveRect.y = position.y + h;

                curveRect.width = fullWidth;
                curveRect.height = h * 3;
            }

            Vector2 curveStart = curveRect.position - new Vector2(0, -curveRect.height);
            Vector2 lastP = curveStart;
            int viewWidth = (int)curveRect.width;

            float os = 0.75f;
            float adrScale = (adsr.attack + adsr.decay + adsr.release) / (viewWidth * os);

            float attackWidth = adsr.attack / adrScale;
            float decayWidth = adsr.decay / adrScale;
            float releaseWidth = adsr.release / adrScale;

            float attackDecayPoint = (adsr.attack + adsr.decay) / (adrScale * os);
            float releaseScale = adsr.release / (adrScale * os);

            const float colorAlpha = 0.4f;


            Rect miniRect = curveRect;
            miniRect.x = (int)miniRect.x;
            miniRect.width = Mathf.CeilToInt(attackWidth);
            EditorGUI.DrawRect(miniRect, new Color(0, 1, 0) * colorAlpha);

            miniRect.x += miniRect.width;
            miniRect.width = Mathf.CeilToInt(decayWidth);
            EditorGUI.DrawRect(miniRect, new Color(1, 1, 0) * colorAlpha);

            miniRect.x += miniRect.width;
            miniRect.width = Mathf.CeilToInt(curveRect.width - (attackWidth + decayWidth + releaseWidth));
            EditorGUI.DrawRect(miniRect, new Color(0, 1, 1) * colorAlpha);

            miniRect.x = Mathf.CeilToInt(curveRect.x + viewWidth - releaseWidth);
            miniRect.width = (int)releaseWidth;
            EditorGUI.DrawRect(miniRect, new Color(1.0f, 0.0f, 1.0f) * colorAlpha);

            float graphScale = adrScale;
            Handles.color = Color.white;
            for (int i = 0; i < viewWidth; i++)
            {
                float v = i < viewWidth - releaseWidth ?
                    adsr.EvaluateIn(i * graphScale) :
                    adsr.EvaluateOut((i - (viewWidth - releaseWidth)) * graphScale);

                Vector2 p = curveStart + new Vector2(i, -v * curveRect.height);
                Handles.DrawLine(lastP, p);
                lastP = p;
            }

            // Doesn't work nicely, not worth it:
            /*
            if (Application.isPlaying)
            {
                float t = adsr.Time;
                float tx = t / adrScale;
                Vector2 cp = curveRect.position;
                cp.x += tx;
                Handles.DrawLine(cp, cp + new Vector2(0, curveRect.height));
            }*/

            if (fold)
            {
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                position.x = startX;
                position.y += curveRect.height + h + 5;
                position.width = fullWidth;

                EditorGUIUtility.labelWidth = 52;

                float margin = 3;
                float propWidth = position.width / 4 - margin;
                var valueRect = new Rect(position.x, position.y, propWidth, h);
                var easeRect = new Rect(position.x + valueRect.width, position.y, valueRect.width, h);

                EditorGUI.PropertyField(valueRect, attackProp); valueRect.x += propWidth + margin;
                EditorGUI.PropertyField(valueRect, decayProp); valueRect.x += propWidth + margin;
                EditorGUI.PropertyField(valueRect, sustainProp); valueRect.x += propWidth + margin;
                EditorGUI.PropertyField(valueRect, releaseProp); valueRect.x = startX; valueRect.y += h;

                EditorGUI.PropertyField(valueRect, attackEaseProp, new GUIContent("Ease")); valueRect.x += propWidth + margin;
                EditorGUI.PropertyField(valueRect, decayEaseProp, new GUIContent("Ease")); valueRect.x += propWidth + margin;
                EditorGUI.PropertyField(valueRect, interruptProp); valueRect.x += propWidth + margin;
                EditorGUI.PropertyField(valueRect, releaseEaseProp, new GUIContent("Ease")); valueRect.x += propWidth + margin;


                if (attackProp.floatValue < 0)
                    attackProp.floatValue = 0;

                if (decayProp.floatValue < 0)
                    decayProp.floatValue = 0;

                if (releaseProp.floatValue < 0)
                    releaseProp.floatValue = 0;

                sustainProp.floatValue = Mathf.Clamp01(sustainProp.floatValue);

                EditorGUI.indentLevel = indent;

                EditorGUIUtility.labelWidth = 0;
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}
