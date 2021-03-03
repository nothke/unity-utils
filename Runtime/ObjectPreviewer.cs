using System.Collections.Generic;
using UnityEngine;

namespace Nothke.Utilities
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