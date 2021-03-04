///
/// RTUtils by Nothke
/// 
/// RenderTexture utilities for direct drawing sprites and converting to Texture2D.
/// Requires BlitQuad shader.
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
    public static class RTUtils
    {
        static Mesh quad;
        public static Mesh GetQuad()
        {
            if (quad)
                return quad;

            Mesh mesh = new Mesh();

            float width = 1;
            float height = 1;

            Vector3[] vertices = new Vector3[4]
            {
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(0, height, 0),
            new Vector3(width, height, 0)
            };
            mesh.vertices = vertices;

            int[] tris = new int[6]
            {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
            };
            mesh.triangles = tris;

            Vector2[] uv = new Vector2[4]
            {
              new Vector2(0, 0),
              new Vector2(1, 0),
              new Vector2(0, 1),
              new Vector2(1, 1)
            };
            mesh.uv = uv;

            quad = mesh;
            return quad;
        }

        static Shader blitShader;
        public static Shader GetBlitShader()
        {
            if (blitShader)
                return blitShader;

            var shader = Shader.Find("Hidden/BlitQuad");

            if (!shader)
                Debug.LogError("BlitTest shader not found, did you forget to include it in the project settings?");

            blitShader = shader;
            return blitShader;
        }

        static Material blitMaterial;
        public static Material GetBlitMaterial()
        {
            if (blitMaterial)
                return blitMaterial;

            return new Material(GetBlitShader());
        }

        const float zNear = 0.1f;
        const float zFar = 100.0f;
        const float zPos = -zFar / 2;

        public static void BlitQuad(RenderTexture rt, Material material, Vector2 position, Vector2 scale)
        {
            Matrix4x4 objectMatrix = Matrix4x4.TRS(
                new Vector3(position.x, position.y, zPos),
                Quaternion.identity, scale);

            // Create an orthographic matrix (for 2D rendering)
            // You can otherwise use Matrix4x4.Perspective()
            Matrix4x4 projectionMatrix = Matrix4x4.Ortho(0, 1, 0, 1, zNear, zFar);

            // This fixes flickering (by @guycalledfrank)
            // (because there's some switching back and forth between cameras, I don't fully understand)
            if (Camera.current != null)
                projectionMatrix *= Camera.current.worldToCameraMatrix.inverse;

            // Remember the current texture and set our own as "active".
            RenderTexture prevRT = RenderTexture.active;
            RenderTexture.active = rt;

            // Set material as "active". Without this, Unity editor will freeze.
            material.SetPass(0);

            // Push the projection matrix
            GL.PushMatrix();
            GL.LoadProjectionMatrix(projectionMatrix);

            // It seems that the faces are in a wrong order, so we need to flip them
            GL.invertCulling = true;

            // Clear the texture
            //GL.Clear(true, true, Color.black);

            // Draw the mesh!
            Graphics.DrawMeshNow(GetQuad(), objectMatrix);

            // Pop the projection matrix to set it back to the previous one
            GL.PopMatrix();
            GL.invertCulling = false;

            // Re-set the RenderTexture to the last used one
            RenderTexture.active = prevRT;
        }

        public static void BlitSprite(RenderTexture rt, Texture texture, Vector2 position, Vector2 scale)
        {
            Material material = GetBlitMaterial();
            material.mainTexture = texture;
            //material.SetTexture("_TargetTex", texture);

            BlitQuad(rt, material, position, scale);
        }

        public static Texture2D ConvertToTexture2D(this RenderTexture rt,
            TextureFormat format = TextureFormat.RGB24,
            FilterMode filterMode = FilterMode.Bilinear)
        {
            Texture2D tex = new Texture2D(rt.width, rt.height, format, false);
            tex.filterMode = filterMode;

            RenderTexture prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

            tex.Apply();

            RenderTexture.active = prevActive;

            return tex;
        }

        public static void DrawTextureGUI(Texture texture)
        {
            GUI.DrawTexture(new Rect(0, 0, texture.width, texture.height), texture);
        }
    }
}