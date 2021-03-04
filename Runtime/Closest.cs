///
/// Closest by Nothke
/// 
/// Extension for finding a closest instance for an array of components.
/// 
/// Example:
///     Clock[] clocks = FindObjectsOfType<Clock>();
///     Clock clock = clocks.GetClosest(transform.position);
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

using System.Collections.Generic;
using UnityEngine;

namespace Nothke.Utils
{
    public static class Closest
    {
        public static T GetClosest<T>(this T[] collection, Vector3 toPoint) where T : Component
        {
            if (collection == null || collection.Length == 0)
                return null;

            T closest = null;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < collection.Length; i++)
            {
                float sqrd = (collection[i].transform.position - toPoint).sqrMagnitude;
                if (sqrd < closestDistance)
                {
                    closestDistance = sqrd;
                    closest = collection[i];
                }
            }

            return closest;
        }

        public static T GetClosest<T>(this IReadOnlyList<T> collection, Vector3 toPoint) where T : Component
        {
            if (collection == null || collection.Count == 0)
                return null;

            T closest = null;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < collection.Count; i++)
            {
                float sqrd = (collection[i].transform.position - toPoint).sqrMagnitude;
                if (sqrd < closestDistance)
                {
                    closestDistance = sqrd;
                    closest = collection[i];
                }
            }

            return closest;
        }
    }
}