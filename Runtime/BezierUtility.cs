///
/// Bezier curve functions, arranged by Nothke,
/// originally found in Unity's URP package
///
/// BezierUtility is a static class with pure functions for sampling curves.
/// BezierCurve is a struct that wraps BezierUtility functions for ease of use.
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
    public struct BezierCurve
    {
        public Vector3 start;
        public Vector3 startTangent;
        public Vector3 endTangent;
        public Vector3 end;

        public BezierCurve(Vector3 start, Vector3 startTangent, Vector3 endTangent, Vector3 end)
        {
            this.start = start;
            this.startTangent = startTangent;
            this.endTangent = endTangent;
            this.end = end;
        }

        public Vector3 ClosestPoint(Vector3 point, out float t, float sqrError = 0.001f)
        {
            Vector3 closest = BezierUtility.ClosestPointOnCurve(
                point,
                start, end, startTangent, endTangent,
                out t, sqrError);

            return closest;
        }

        public Vector3 PointAt(float t)
        {
            return BezierUtility.BezierPoint(
                start, startTangent, endTangent, end, t);
        }

        public Vector3 TangentAt(float t)
        {
            return BezierUtility.BezierTangent(
                start, startTangent, endTangent, end, t);
        }
    }

    public static class BezierUtility
    {
        static Vector3[] s_TempPoints = new Vector3[3];

        public static Vector3 BezierPoint(Vector3 startPosition, Vector3 startTangent, Vector3 endTangent, Vector3 endPosition, float t)
        {
            float s = 1.0f - t;
            return startPosition * s * s * s + startTangent * s * s * t * 3.0f + endTangent * s * t * t * 3.0f + endPosition * t * t * t;
        }

        public static Vector3 BezierTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 C1 = (d - (3.0f * c) + (3.0f * b) - a);
            Vector3 C2 = ((3.0f * c) - (6.0f * b) + (3.0f * a));
            Vector3 C3 = ((3.0f * b) - (3.0f * a));
            //Vector3 C4 = (a);

            return (3.0f * C1 * t * t) + (2.0f * C2 * t) + C3;
        }

        public static Vector3 ClosestPointOnCurve(Vector3 point, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, out float t, float sqrError = 0.001f)
        {
            Vector3 startToEnd = endPosition - startPosition;
            Vector3 startToTangent = (startTangent - startPosition);
            Vector3 endToTangent = (endTangent - endPosition);

            if (Colinear(startToTangent, startToEnd, sqrError) && Colinear(endToTangent, startToEnd, sqrError))
                return ClosestPointToSegment(point, startPosition, endPosition, out t);

            Vector3 leftStartPosition;
            Vector3 leftEndPosition;
            Vector3 leftStartTangent;
            Vector3 leftEndTangent;

            Vector3 rightStartPosition;
            Vector3 rightEndPosition;
            Vector3 rightStartTangent;
            Vector3 rightEndTangent;

            float leftStartT = 0f;
            float leftEndT = 0.5f;
            float rightStartT = 0.5f;
            float rightEndT = 1f;

            SplitBezier(0.5f, startPosition, endPosition, startTangent, endTangent,
                out leftStartPosition, out leftEndPosition, out leftStartTangent, out leftEndTangent,
                out rightStartPosition, out rightEndPosition, out rightStartTangent, out rightEndTangent);

            Vector3 pointLeft = ClosestPointOnCurveIterative(point, leftStartPosition, leftEndPosition, leftStartTangent, leftEndTangent, sqrError, ref leftStartT, ref leftEndT);
            Vector3 pointRight = ClosestPointOnCurveIterative(point, rightStartPosition, rightEndPosition, rightStartTangent, rightEndTangent, sqrError, ref rightStartT, ref rightEndT);

            if ((point - pointLeft).sqrMagnitude < (point - pointRight).sqrMagnitude)
            {
                t = leftStartT;
                return pointLeft;
            }

            t = rightStartT;
            return pointRight;
        }

        public static Vector3 ClosestPointOnCurveFast(Vector3 point, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, out float t)
        {
            float sqrError = 0.001f;
            float startT = 0f;
            float endT = 1f;

            Vector3 closestPoint = ClosestPointOnCurveIterative(point, startPosition, endPosition, startTangent, endTangent, sqrError, ref startT, ref endT);

            t = startT;

            return closestPoint;
        }

        private static Vector3 ClosestPointOnCurveIterative(Vector3 point, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, float sqrError, ref float startT, ref float endT)
        {
            while ((startPosition - endPosition).sqrMagnitude > sqrError)
            {
                Vector3 startToEnd = endPosition - startPosition;
                Vector3 startToTangent = (startTangent - startPosition);
                Vector3 endToTangent = (endTangent - endPosition);

                if (Colinear(startToTangent, startToEnd, sqrError) && Colinear(endToTangent, startToEnd, sqrError))
                {
                    float t;
                    Vector3 closestPoint = ClosestPointToSegment(point, startPosition, endPosition, out t);
                    t *= (endT - startT);
                    startT += t;
                    endT -= t;
                    return closestPoint;
                }

                Vector3 leftStartPosition;
                Vector3 leftEndPosition;
                Vector3 leftStartTangent;
                Vector3 leftEndTangent;

                Vector3 rightStartPosition;
                Vector3 rightEndPosition;
                Vector3 rightStartTangent;
                Vector3 rightEndTangent;

                SplitBezier(0.5f, startPosition, endPosition, startTangent, endTangent,
                    out leftStartPosition, out leftEndPosition, out leftStartTangent, out leftEndTangent,
                    out rightStartPosition, out rightEndPosition, out rightStartTangent, out rightEndTangent);

                s_TempPoints[0] = leftStartPosition;
                s_TempPoints[1] = leftStartTangent;
                s_TempPoints[2] = leftEndTangent;

                float sqrDistanceLeft = SqrDistanceToPolyLine(point, s_TempPoints);

                s_TempPoints[0] = rightEndPosition;
                s_TempPoints[1] = rightEndTangent;
                s_TempPoints[2] = rightStartTangent;

                float sqrDistanceRight = SqrDistanceToPolyLine(point, s_TempPoints);

                if (sqrDistanceLeft < sqrDistanceRight)
                {
                    startPosition = leftStartPosition;
                    endPosition = leftEndPosition;
                    startTangent = leftStartTangent;
                    endTangent = leftEndTangent;

                    endT -= (endT - startT) * 0.5f;
                }
                else
                {
                    startPosition = rightStartPosition;
                    endPosition = rightEndPosition;
                    startTangent = rightStartTangent;
                    endTangent = rightEndTangent;

                    startT += (endT - startT) * 0.5f;
                }
            }

            return endPosition;
        }

        public static void SplitBezier(float t, Vector3 startPosition, Vector3 endPosition, Vector3 startRightTangent, Vector3 endLeftTangent,
            out Vector3 leftStartPosition, out Vector3 leftEndPosition, out Vector3 leftStartTangent, out Vector3 leftEndTangent,
            out Vector3 rightStartPosition, out Vector3 rightEndPosition, out Vector3 rightStartTangent, out Vector3 rightEndTangent)
        {
            Vector3 tangent0 = (startRightTangent - startPosition);
            Vector3 tangent1 = (endLeftTangent - endPosition);
            Vector3 tangentEdge = (endLeftTangent - startRightTangent);

            Vector3 tangentPoint0 = startPosition + tangent0 * t;
            Vector3 tangentPoint1 = endPosition + tangent1 * (1f - t);
            Vector3 tangentEdgePoint = startRightTangent + tangentEdge * t;

            Vector3 newTangent0 = tangentPoint0 + (tangentEdgePoint - tangentPoint0) * t;
            Vector3 newTangent1 = tangentPoint1 + (tangentEdgePoint - tangentPoint1) * (1f - t);
            Vector3 newTangentEdge = newTangent1 - newTangent0;

            Vector3 bezierPoint = newTangent0 + newTangentEdge * t;

            leftStartPosition = startPosition;
            leftEndPosition = bezierPoint;
            leftStartTangent = tangentPoint0;
            leftEndTangent = newTangent0;

            rightStartPosition = bezierPoint;
            rightEndPosition = endPosition;
            rightStartTangent = newTangent1;
            rightEndTangent = tangentPoint1;
        }

        private static Vector3 ClosestPointToSegment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd, out float t)
        {
            Vector3 relativePoint = point - segmentStart;
            Vector3 segment = (segmentEnd - segmentStart);
            Vector3 segmentDirection = segment.normalized;
            float length = segment.magnitude;

            float dot = Vector3.Dot(relativePoint, segmentDirection);

            if (dot <= 0f)
                dot = 0f;
            else if (dot >= length)
                dot = length;

            t = dot / length;

            return segmentStart + segment * t;
        }

        private static float SqrDistanceToPolyLine(Vector3 point, Vector3[] points)
        {
            float minDistance = float.MaxValue;

            for (int i = 0; i < points.Length - 1; ++i)
            {
                float distance = SqrDistanceToSegment(point, points[i], points[i + 1]);

                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }

        private static float SqrDistanceToSegment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
        {
            Vector3 relativePoint = point - segmentStart;
            Vector3 segment = (segmentEnd - segmentStart);
            Vector3 segmentDirection = segment.normalized;
            float length = segment.magnitude;

            float dot = Vector3.Dot(relativePoint, segmentDirection);

            if (dot <= 0f)
                return (point - segmentStart).sqrMagnitude;
            else if (dot >= length)
                return (point - segmentEnd).sqrMagnitude;

            return Vector3.Cross(relativePoint, segmentDirection).sqrMagnitude;
        }

        private static bool Colinear(Vector3 v1, Vector3 v2, float error = 0.0001f)
        {
            return Mathf.Abs(v1.x * v2.y - v1.y * v2.x + v1.x * v2.z - v1.z * v2.x + v1.y * v2.z - v1.z * v2.y) < error;
        }
    }
}
