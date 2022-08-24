///
/// PID.cs by Nothke
///
/// Struct based PID Controllers for float, Vector2 and Vector3
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
    public struct PIDFloat
    {
        public float pFactor, iFactor, dFactor;
        float integral, lastError;

        public float Update(float setpoint, float actual, float timeFrame)
        {
            float present = setpoint - actual;
            integral += present * timeFrame;
            float deriv = (present - lastError) / timeFrame;
            lastError = present;
            return present * pFactor + integral * iFactor + deriv * dFactor;
        }
    }

    [System.Serializable]
    public struct PIDVector2
    {
        public float pFactor, iFactor, dFactor;
        Vector2 integral, lastError;

        public Vector2 Update(Vector2 setpoint, Vector2 actual, float timeFrame)
        {
            Vector2 present = setpoint - actual;
            integral += present * timeFrame;
            Vector2 deriv = (present - lastError) / timeFrame;
            lastError = present;
            return present * pFactor + integral * iFactor + deriv * dFactor;
        }
    }

    [System.Serializable]
    public struct PIDVector3
    {
        public float pFactor, iFactor, dFactor;
        Vector3 integral, lastError;

        public Vector3 Update(Vector3 setpoint, Vector3 actual, float timeFrame)
        {
            Vector3 present = setpoint - actual;
            integral += present * timeFrame;
            Vector3 deriv = (present - lastError) / timeFrame;
            lastError = present;
            return present * pFactor + integral * iFactor + deriv * dFactor;
        }
    }
}