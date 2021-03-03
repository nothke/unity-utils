using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelDrawer : MonoBehaviour
{
    struct Label3D
    {
        public readonly string text;
        public readonly Vector3 position;

        public Label3D(string text, Vector3 position)
        {
            this.text = text;
            this.position = position;
        }
    }

    List<Label3D> labels = new List<Label3D>();

    public static LabelDrawer e;
    void Awake()
    {
        e = this;

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
            e.labels.Add(new Label3D(text, position));
    }

    private void OnDisable()
    {
        labels.Clear();
    }


    private void OnGUI()
    {
        Camera cam = Camera.main;

        foreach (var label in labels)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(label.position);
            if (screenPos.z < 0) // if behind
                continue;

            GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 100, 100), label.text);
        }
    }
}