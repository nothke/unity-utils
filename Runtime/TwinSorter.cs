///
/// TwinSort by Nothke
/// 
/// Sorts a "target" list by values in the "sorter" list.
/// Useful when having a cache of precomputed values by which you want to sort another list,
/// avoiding the use of cpu/memory hungry delegates or Linq.
/// 
/// Sorter list needs to be IComparable, but the Target doesn't need to be
/// 
/// Usage: myTargetList.TwinSort(valuesToUseForSortingList);
/// 
/// Example: Sorting a list of Seat components by distance to player
/// 
/// List<Seat> cachedSeats;
/// List<float> cachedDistances;
/// 
/// public void SortSeatsByDistanceToPlayer()
/// {
///     // Just an example, you'd prefer to keep a cached list yourself instead of using FindObjectsOfType:
///     FindObjectsOfType<Seat>(cachedSeats); 
///     cachedDistances.Clear();
///     
///     Vector3 playerPos = transform.position;
///     
///     // Calculate distances and put in a list
///     for (int i = 0; i < cachedSeats.Count; i++)
///     {
///         cachedDistances.Add(cachedSeats[i].transform.position - playerPos).sqrMagnitude);
///     }
///     
///     cachedSeats.TwinSort(cachedDistances);
///     
///     for (int i = 0; i < cachedSeats.Count; i++)
///          // ...Use the sorted data
/// }
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nothke.Utils
{
    /// <summary>
    /// Sorts a target list by values in the sorter list.
    /// Useful when having a cache of precomputed values by which you want to sort another list.
    /// (..And to avoid having to use Linq and delegates)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public static class TwinSorter
    {
        public static void TwinSort<T, U>(this List<T> target, List<U> sorter) where U : System.IComparable<U>
        {
            if (target.Count != sorter.Count)
                throw new System.Exception("TwinListSort lists must be of equal count.");

            qSort(target, sorter, 0, target.Count - 1);
        }

        private static void qSort<T, U>(List<T> inputs, List<U> toSort, int left, int right) where U : System.IComparable<U>
        {
            //set the indexes
            int leftIndex = left;
            int rightIndex = right;

            //get the pivot
            var pivot = toSort[left + (right - left) / 2];
            while (leftIndex <= rightIndex)
            {
                //check left values
                while (toSort[leftIndex].CompareTo(pivot) < 0)
                    leftIndex++;

                //check right values
                while (toSort[rightIndex].CompareTo(pivot) > 0)
                    rightIndex--;

                //swap
                if (leftIndex <= rightIndex)
                {
                    var tmp = toSort[leftIndex];
                    toSort[leftIndex] = toSort[rightIndex];
                    toSort[rightIndex] = tmp;

                    // swap the target list as well
                    var tmpi = inputs[leftIndex];
                    inputs[leftIndex] = inputs[rightIndex];
                    inputs[rightIndex] = tmpi;

                    //move towards pivot
                    leftIndex++;
                    rightIndex--;
                }
            }
            
            //continues to sort left and right of pivot
            if (left < rightIndex)
                qSort(inputs, toSort, left, rightIndex);

            if (leftIndex < right)
                qSort(inputs, toSort, leftIndex, right);
        }
    }
}