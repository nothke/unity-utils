using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nothke.Utilities
{
    public static class GizmosX
    {
        public static void DrawCircle(Vector3 pos, Vector3 forward, float radius)
        {
#if UNITY_EDITOR
            Color c = Handles.color;
            Handles.color = Gizmos.color;

            Handles.CircleHandleCap(
                -1, pos, Quaternion.LookRotation(forward), radius, EventType.Repaint);

            Handles.color = c;
#endif
        }

        public static void DrawArc(Vector2 center, Vector2 startDir, Vector2 endDir, float radius)
        {
#if UNITY_EDITOR
            Color c = Handles.color;
            Handles.color = Gizmos.color;

            float angle = Vector2.SignedAngle(startDir.normalized, endDir.normalized);
            if (angle > 0) angle -= 360;
            UnityEditor.Handles.DrawWireArc(center, Vector3.forward, startDir, angle, radius);

            Handles.color = c;
#endif
        }

        public static void DrawLabel(Vector3 position, string label)
        {
#if UNITY_EDITOR
            Color c = Handles.color;
            Handles.color = Gizmos.color;

            Handles.Label(position, label);

            Handles.color = c;
#endif
        }

        /// <summary>
        /// A wire cube with rotation!
        /// </summary>
        public static void DrawRotatedWireCube(Vector3 position, Quaternion rotation, Vector3 size)
        {
#if UNITY_EDITOR
            Vector3 extents = size * 0.5f;

            Vector3 p000 = position + rotation * new Vector3(-size.x, -size.y, -size.z);
            Vector3 p100 = position + rotation * new Vector3(size.x, -size.y, -size.z);
            Vector3 p010 = position + rotation * new Vector3(-size.x, size.y, -size.z);
            Vector3 p110 = position + rotation * new Vector3(size.x, size.y, -size.z);

            Vector3 p001 = position + rotation * new Vector3(-size.x, -size.y, size.z);
            Vector3 p101 = position + rotation * new Vector3(size.x, -size.y, size.z);
            Vector3 p011 = position + rotation * new Vector3(-size.x, size.y, size.z);
            Vector3 p111 = position + rotation * new Vector3(size.x, size.y, size.z);

            // Forward facing lines
            Gizmos.DrawLine(p000, p001);
            Gizmos.DrawLine(p100, p101);
            Gizmos.DrawLine(p010, p011);
            Gizmos.DrawLine(p110, p111);

            // Up facing lines
            Gizmos.DrawLine(p000, p010);
            Gizmos.DrawLine(p100, p110);
            Gizmos.DrawLine(p001, p011);
            Gizmos.DrawLine(p101, p111);

            Gizmos.DrawLine(p000, p100);
            Gizmos.DrawLine(p010, p110);
            Gizmos.DrawLine(p001, p101);
            Gizmos.DrawLine(p011, p111);
#endif
        }

        /// <summary>
        /// Draws a planar heightmap with provided 2D heights array
        /// </summary>
        public static void DrawHeightmap(Vector3 rootPos, ref float[,] heights, float separation, float heightMult = 1)
        {
#if UNITY_EDITOR
            Vector3 up = Vector3.up;

            int sizeX = heights.GetLength(0);
            int sizeY = heights.GetLength(1);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    Vector3 pos = rootPos + new Vector3(
                        x * separation,
                        heights[x, y] * heightMult,
                        y * separation);

                    Vector3 prevXPos = x != 0 ?
                        rootPos + new Vector3(
                        (x - 1) * separation,
                        heights[x - 1, y] * heightMult,
                        y * separation) :
                        up;

                    Vector3 prevYPos = y != 0 ?
                        rootPos + new Vector3(
                        x * separation,
                        heights[x, y - 1] * heightMult,
                        (y - 1) * separation) :
                        up;

                    if (x != 0) Gizmos.DrawLine(pos, prevXPos);
                    if (y != 0) Gizmos.DrawLine(pos, prevYPos);
                    //Gizmos.DrawLine(pos, up * heights[x, y]);
                }
            }
#endif
        }

        public static void DrawGridGradient(Vector3 rootPos, ref float[,] heights, float separation,
            Gradient gradient, float colorPreviewHeightMult, float heightMult)
        {
#if UNITY_EDITOR
            Vector3 up = Vector3.up;

            int sizeX = heights.GetLength(0);
            int sizeY = heights.GetLength(1);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    Vector3 pos = rootPos + new Vector3(
                        x * separation,
                        heights[x, y] * heightMult,
                        y * separation);

                    Vector3 prevXPos = x != 0 ?
                        rootPos + new Vector3(
                        (x - 1) * separation,
                        heights[x - 1, y] * heightMult,
                        y * separation) :
                        up;

                    Vector3 prevYPos = y != 0 ?
                        rootPos + new Vector3(
                        x * separation,
                        heights[x, y - 1] * heightMult,
                        (y - 1) * separation) :
                        up;

                    Gizmos.color = gradient.Evaluate(pos.y * colorPreviewHeightMult);
                    if (x != 0) Gizmos.DrawLine(pos, prevXPos);
                    if (y != 0) Gizmos.DrawLine(pos, prevYPos);
                    //Gizmos.DrawLine(pos, up * heights[x, y]);
                }
            }
#endif
        }

        /// <summary>
        /// Draws a planar grid from 2D heights array as vertical height lines
        /// </summary>
        public static void DrawGridLineHeights(Vector3 rootPos, ref float[,] heights, float separation)
        {
#if UNITY_EDITOR
            Vector3 up = Vector3.up;

            int sizeX = heights.GetLength(0);
            int sizeY = heights.GetLength(1);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    Vector3 pos = rootPos + new Vector3(
                        x * separation,
                        0,
                        y * separation);

                    Gizmos.DrawRay(pos, up * heights[x, y]);
                }
            }
#endif
        }

        /// <summary>
        /// Draws a lattice - 3D grid
        /// </summary>
        public static void DrawLattice(Vector3 point, Vector3Int dimension, Vector3 separation)
        {
#if UNITY_EDITOR
            float sx = separation.x;
            float sy = separation.y;
            float sz = separation.z;

            for (int y = 0; y < dimension.y + 1; y++)
            {
                for (int x = 0; x < dimension.x + 1; x++)
                {
                    Gizmos.DrawLine(
                        point + new Vector3(x * sx, y * sy, 0),
                        point + new Vector3(x * sx, y * sy, dimension.z * sz));
                }

                for (int z = 0; z < dimension.z + 1; z++)
                {
                    Gizmos.DrawLine(
                        point + new Vector3(0, y * sy, z * sz),
                        point + new Vector3(dimension.x * sx, y * sy, z * sz));
                }
            }

            for (int x = 0; x < dimension.x + 1; x++)
            {
                for (int z = 0; z < dimension.z + 1; z++)
                {
                    Gizmos.DrawLine(
                        point + new Vector3(x * sx, 0, z * sz),
                        point + new Vector3(x * sx, dimension.y * sy, z * sz));
                }
            }
#endif
        }
    }
}