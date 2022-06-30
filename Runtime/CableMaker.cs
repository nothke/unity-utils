///
/// CableMaker by Nothke
/// 
/// Builds LineRenderer-based hanging cables using catenary formula.
/// 
/// Also has a static function CreateCatenary() for filling up a list of
/// catenary points so you can use it with any other rendering system.
/// 
/// Way it works as a component is it creates a LineRenderer on Start().
///
/// Thanks to Alan Zucconi's post for different height ends and constant length:
/// https://www.alanzucconi.com/2020/12/13/catenary-1/
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
using System.Collections.Generic;

namespace Nothke.Utils
{
    public class CableMaker : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("cableStart")]
        public Transform start;
        [UnityEngine.Serialization.FormerlySerializedAs("cableEnd")]
        public Transform end;

        [Min(1.0f)] public int segments = 9;
        [Min(0)] public float slack = 0.2f;

        public Material material;
        [Min(0)] public float width = 0.03f;

        LineRenderer line;

        [System.NonSerialized]
        public List<Vector3> linePoints = new List<Vector3>();

        void Start()
        {
            Generate();
        }

        #region Public Methods

        public bool BothEndsExist()
        {
            return start && end;
        }

        public void Generate()
        {
            if (!BothEndsExist())
                return;

            UpdateCatenary();
            SetToLineRenderer();
        }

        public void UpdateCatenary()
        {
            if (!BothEndsExist())
                return;

            float length = (end.position - start.position).magnitude;
            float targetLength = length + slack;

            CreateCatenary(linePoints, start.position, end.position, segments, targetLength);
        }

        public void SetToLineRenderer()
        {
            line = GetComponent<LineRenderer>();

            if (!line)
            {
                line = gameObject.AddComponent<LineRenderer>();
                line.useWorldSpace = false;
                line.material = material;

                line.startWidth = width;
                line.endWidth = width;
            }

            line.positionCount = linePoints.Count;

            for (int i = 0; i < linePoints.Count; i++)
                line.SetPosition(i, transform.InverseTransformPoint(linePoints[i]));
        }

        #endregion

        #region Static Methods

        static float Cosh(float f) => (float)System.Math.Cosh(f);
        static float Sinh(float f) => (float)System.Math.Sinh(f);
        static float Coth(float f) => Cosh(f) / Sinh(f);

        /// <summary>
        /// Fills up a list of points with catenary curve from p1 to p2.
        /// If the distance between ends is greater than targetLength, returns just a p1-p2 line.
        /// </summary>
        /// <param name="points">List of points to fill up. Will be overwritten. Must not be null.</param>
        public static void CreateCatenary(List<Vector3> points, in Vector3 p1, in Vector3 p2, int segments, float targetLength)
        {
            if (segments < 1)
                segments = 1;

            Vector3 diff = p2 - p1;

            // Fully taut
            if (segments == 1 || diff.magnitude > targetLength)
            {
                points.Clear();
                points.Add(p1);
                points.Add(p2);
                return;
            }

            float yDiff = diff.y;
            Vector3 planarDiff = diff;
            planarDiff.y = 0;
            float xDiff = planarDiff.magnitude;

            float l = targetLength;

            // Simple step method for finding 'a' from: https://www.alanzucconi.com/2020/12/13/catenary-2/

            const float step = 0.001f;
            float s = 0;
            do
                s += step;
            while (Sinh(s) / s < Mathf.Sqrt(l * l - yDiff * yDiff) / xDiff);

            float a = xDiff / s / 2.0f;
            float p = (xDiff - a * Mathf.Log((l + yDiff) / (l - yDiff))) / 2.0f;
            float q = (yDiff - l * Coth(s)) / 2.0f;

            points.Clear();

            for (int i = 0; i < segments + 1; i++)
            {
                float x = (float)i / segments;

                // from: https://en.wikipedia.org/wiki/Catenary#Determining_parameters
                float y = a * Cosh((x * xDiff - p) / a) + q;

                Vector3 point = planarDiff * x;
                point.y = y;
                points.Add(p1 + point);
            }
        }

        #endregion

#if UNITY_EDITOR
        static readonly Vector3 gizmosCubeSize = Vector3.one * 0.1f;

        private void OnValidate()
        {
            UpdateCatenary();
        }

        void DrawGizmoEnds()
        {
            if (!start || !end)
            {
                Gizmos.color = Color.red;

                if (end)
                    Gizmos.DrawCube(end.position, gizmosCubeSize);
                else if (start)
                    Gizmos.DrawCube(start.position, gizmosCubeSize);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(start.position, gizmosCubeSize);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(end.position, gizmosCubeSize);
            }
        }

        void DrawCableGizmos()
        {
            if (Application.isPlaying || !BothEndsExist())
                return;

            for (int i = 0; i < linePoints.Count - 1; i++)
                Gizmos.DrawLine(linePoints[i], linePoints[i + 1]);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 0.5f);
            DrawCableGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            UpdateCatenary();
            DrawGizmoEnds();

            Gizmos.color = Color.white;
            DrawCableGizmos();
        }
#endif
    }
}