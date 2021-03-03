using System.Collections.Generic;
using UnityEngine;

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

    public static T GetClosest<T>(this List<T> collection, Vector3 toPoint) where T : Component
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
