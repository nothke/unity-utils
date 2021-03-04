using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nothke.Utils
{
    public static class RandomExtensions
    {
        public static T GetRandom<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        public static T GetRandom<T>(this IReadOnlyList<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }
    }
}