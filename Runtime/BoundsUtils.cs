///
/// BoundsUtils by Nothke
/// 
/// Additional utils for bounds like finding bounds of an entire GameObject in
/// world or local space, collider bounds, transforming bounds, etc.
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
    public static class BoundsUtils
    {
        // static lists for caching, never deallocates
        static List<Collider> colliderCache = new List<Collider>();
        static List<MeshFilter> meshFilterCache = new List<MeshFilter>();

        /// <summary>
        /// Gets bounds of a uniion of all object's child colliders relative to the object
        /// </summary>
        public static Bounds GetObjectSpaceColliderBounds(GameObject go, bool includeInactive = false)
        {
            var rootW2L = go.transform.worldToLocalMatrix;

            go.GetComponentsInChildren(includeInactive, colliderCache);

            if (colliderCache.Count == 0)
            {
                Debug.LogError("Attempting to get bounds of the object but it has no colliders");
                return default;
            }

            Bounds goBounds = GetBoundsInRootSpace(colliderCache[0]);

            for (int i = 1; i < colliderCache.Count; i++)
            {
                Bounds b = GetBoundsInRootSpace(colliderCache[i]);
                goBounds.Encapsulate(b);
            }

            return goBounds;

            Bounds GetBoundsInRootSpace(Collider col)
            {
                Bounds b = col.GetLocalBounds();
                Matrix4x4 l2w = col.transform.localToWorldMatrix;
                Matrix4x4 local = rootW2L * l2w;
                return TransformBounds(local, b);
            }
        }

        public static Bounds GetObjectSpaceRendererBounds(GameObject go, bool includeInactive = false)
        {
            var rootW2L = go.transform.worldToLocalMatrix;

            go.GetComponentsInChildren(includeInactive, meshFilterCache);

            if (meshFilterCache.Count == 0)
            {
                Debug.LogError("Attempting to get bounds of the object but it has no renderers");
                return default;
            }

            Bounds goBounds = GetBoundsInRootSpace(meshFilterCache[0]);

            for (int i = 1; i < meshFilterCache.Count; i++)
            {
                Bounds b = GetBoundsInRootSpace(meshFilterCache[i]);
                goBounds.Encapsulate(b);
            }

            return goBounds;

            Bounds GetBoundsInRootSpace(MeshFilter mf)
            {
                Bounds b = mf.sharedMesh.bounds;
                Matrix4x4 l2w = mf.transform.localToWorldMatrix;
                Matrix4x4 local = rootW2L * l2w;
                return TransformBounds(local, b);
            }
        }

        /// <summary>
        /// Finds bounds of a collider in local space
        /// </summary>
        public static Bounds GetLocalBounds(this Collider collider)
        {
            if (collider is BoxCollider)
            {
                BoxCollider box = (BoxCollider)collider;
                return new Bounds(box.center, box.size);
            }
            else if (collider is SphereCollider)
            {
                var center = ((SphereCollider)collider).center;
                var radius = ((SphereCollider)collider).radius;
                Vector3 size = new Vector3(radius * 2, radius * 2, radius * 2);
                return new Bounds(center, size);
            }
            else if (collider is CapsuleCollider)
            {
                var capsule = (CapsuleCollider)collider;
                var r = capsule.radius;
                var h = capsule.height;

                Vector3 size;
                switch (capsule.direction)
                {
                    case 0: size = new Vector3(h, r * 2, r * 2); break;
                    case 1: size = new Vector3(r * 2, h, r * 2); break;
                    case 2: size = new Vector3(r * 2, r * 2, h); break;
                    default: size = default; break;
                }

                return new Bounds(capsule.center, size);
            }
            else if (collider is MeshCollider)
            {
                return ((MeshCollider)collider).sharedMesh.bounds;
            }

            Debug.LogError("Attempting to get bounds of an unknown collider type");
            return new Bounds();
        }

        public static Bounds TransformBounds(in Matrix4x4 mat, in Bounds bounds)
        {
            // Find 8 corners of the bounds
            Vector3 p0 = bounds.min;
            Vector3 p1 = bounds.max;
            Vector3 p2 = new Vector3(p0.x, p0.y, p1.z);
            Vector3 p3 = new Vector3(p0.x, p1.y, p0.z);
            Vector3 p4 = new Vector3(p1.x, p0.y, p0.z);
            Vector3 p5 = new Vector3(p0.x, p1.y, p1.z);
            Vector3 p6 = new Vector3(p1.x, p0.y, p1.z);
            Vector3 p7 = new Vector3(p1.x, p1.y, p0.z);

            Bounds b = new Bounds(mat * p0, Vector3.zero);
            b.Encapsulate(mat.MultiplyPoint(p1));
            b.Encapsulate(mat.MultiplyPoint(p2));
            b.Encapsulate(mat.MultiplyPoint(p3));
            b.Encapsulate(mat.MultiplyPoint(p4));
            b.Encapsulate(mat.MultiplyPoint(p5));
            b.Encapsulate(mat.MultiplyPoint(p6));
            b.Encapsulate(mat.MultiplyPoint(p7));

            return b;
        }

        public static void DrawBoundsGizmos(in Bounds bounds)
        {
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}