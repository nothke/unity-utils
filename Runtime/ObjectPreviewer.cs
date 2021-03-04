///
/// ObjectPreviewer by Nothke
/// 
/// Directly draws a GameObject for preview without any object cloning.
/// Used for example when you want to preview the "build" location in RTS games.
/// 
/// How to use:
/// 
/// 1. Call ObjectPreviewer.SetObject() once, when you switch objects
/// 2. Call ObjectPreviewer.Render() every frame you want your previously set
/// object to be rendered.
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
    public static class ObjectPreviewer
    {
        struct Node
        {
            public Matrix4x4 transform;
            public Mesh mesh;
            public Material[] mats;
        }

        // Static cache
        static List<MeshFilter> meshFiltersBuffer;
        static List<Node> nodes;

        /// <summary>
        /// For performance reasons, the cache never deallocates.
        /// So, call this to clear the cache only in the case the memory becomes a problem, such as with previewing objects with gigantic hierarchies.
        /// (but even if they're gigantic it's quite unlikely it will be a problem)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ReloadCache()
        {
            meshFiltersBuffer = new List<MeshFilter>();
            nodes = new List<Node>();
        }

        /// <summary>
        /// Assigns object for rendering. Call this only once on preview object change. Pass null to clear the object.
        /// </summary>
        public static void SetObject(GameObject go)
        {
            nodes.Clear();
            meshFiltersBuffer.Clear();

            if (go == null)
                return;

            Matrix4x4 rootW2L = go.transform.worldToLocalMatrix;
            // We need to scale in case the root has non 1,1,1 scale
            Matrix4x4 rootScaleMatrix = Matrix4x4.Scale(go.transform.localScale);
            go.transform.GetComponentsInChildren(meshFiltersBuffer);

            foreach (var mf in meshFiltersBuffer)
            {
                Mesh mesh = mf.sharedMesh;
                var mr = mf.GetComponent<MeshRenderer>();

                if (mesh == null)
                    continue;

                // Un-transform by root
                Matrix4x4 matrix = rootW2L * mf.transform.localToWorldMatrix * rootScaleMatrix;

                Material[] mats = null;
                if (mr != null)
                    mats = mr.sharedMaterials;

                nodes.Add(new Node()
                {
                    mesh = mesh,
                    transform = matrix,
                    mats = mats,
                });
            }
        }

        /// <summary>
        /// Renders the preview object set with SetObject(). Call this every frame you want the object to be drawn.
        /// </summary>
        /// <param name="overrideMaterial">The replacement material that the previews will be drawn with. If not assigned, it will use the original material.</param>
        public static void Render(Vector3 position, Quaternion rotation, Vector3 scale, Material overrideMaterial = null, int renderLayer = 0)
        {
            Matrix4x4 previewTransform = Matrix4x4.TRS(position, rotation, scale);

            foreach (var node in nodes)
            {
                for (int subMeshIndex = 0; subMeshIndex < node.mesh.subMeshCount; subMeshIndex++)
                {
                    var mat = overrideMaterial != null ? overrideMaterial : node.mats[Mathf.Clamp(subMeshIndex, 0, node.mats.Length)];
                    Graphics.DrawMesh(node.mesh, previewTransform * node.transform, mat, renderLayer, null, subMeshIndex);
                }
            }
        }
    }
}