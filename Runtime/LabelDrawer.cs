///
/// LabelDrawer by Nothke
/// 
/// Draws labels at a 3D position at runtime while LabelDrawer.Label() is called.
/// 
/// Do not call from OnDrawGizmos, this is intended for runtime,
/// see GizmosX.cs instead.
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

public class LabelDrawer : MonoBehaviour
{
    public Font font;

    struct Label3D
    {
        public readonly string text;
        public readonly Vector3 position;
        public readonly Color color;

        public Label3D(string text, Vector3 position, Color color)
        {
            this.text = text;
            this.position = position;
            this.color = color;
        }
    }

    List<Label3D> labels = new List<Label3D>();

    static readonly Color defaultColor = Color.white;
    GUIStyle style = new GUIStyle();

    public static LabelDrawer e;
    void Awake()
    {
        e = this;

        if (font)
            style.font = font;

        style.normal.textColor = defaultColor;

        // Needs to wait for end of frame because it's the only event that happens after OnGUI
        StartCoroutine(EndOfFrameLoop());
    }

    WaitForEndOfFrame waifu = new WaitForEndOfFrame();

    IEnumerator EndOfFrameLoop()
    {
        while (true)
        {
            yield return waifu;
            labels.Clear();
        }
    }

    static void CreateIfDoesntExist()
    {
        if (!e)
        {
            var go = new GameObject("-- LabelDrawer");
            var drawer = go.AddComponent<LabelDrawer>();
            e = drawer;
        }
    }


    public static void Label(string text, Vector3 position)
    {
        CreateIfDoesntExist();

        if (e.enabled)
            e.labels.Add(new Label3D(text, position, defaultColor));
    }

    public static void Label(string text, Vector3 position, Color color)
    {
        CreateIfDoesntExist();

        if (e.enabled)
            e.labels.Add(new Label3D(text, position, color));
    }

    private void OnDisable()
    {
        labels.Clear();
    }


    private void OnGUI()
    {
        Camera cam = Camera.main;

        Color originalContentColor = GUI.contentColor;

        foreach (var label in labels)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(label.position);
            if (screenPos.z < 0) // if behind
                continue;

            GUI.contentColor = label.color;
            GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 1000, 1000), label.text, style);
        }

        GUI.contentColor = originalContentColor;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        bool isGameView = UnityEditor.SceneView.currentDrawingSceneView == null;

        if (Application.isPlaying && !isGameView && e && e.enabled)
        {
            if (Application.isEditor)
            {
                foreach (var label in labels)
                {
                    style.normal.textColor = label.color;
                    UnityEditor.Handles.Label(label.position, label.text, style);
                }
            }

            // Required because WaitForEndOfFrame doesn't work when Game view is not visible
            // When both Game and Scene view are visible at the same time tho, it should only clear when Game is not in focus
            if (!Application.isFocused)
                labels.Clear();
        }
    }
#endif
}