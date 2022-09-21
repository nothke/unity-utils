///
/// Pivot.cs by Nothke
///
/// Struct based wrapper for relative position and rotation.
/// Used to avoid sub-GameObjects for relative transforms.
/// 
/// Usage:
/// To use, add it to your script as:
/// 
/// public Pivot pivot;
/// 
/// It will show as editable struct in the inspector.
/// To see it in scene view, in your script's OnDrawGizmos, call:
/// 
/// pivot.DrawGizmos(transform);
/// 
/// But to be able to manipulate it in scene view,
/// you unfortunatelly MUST make a custom editor for it,
/// and call Pivot.DrawHandles(property, transform).
/// However, that could be as short as:
/// 
/// #if UNITY_EDITOR
/// [CustomEditor(typeof(TestPivot))]
/// public class TestPivotEditor : Editor
/// {
///     private void OnSceneGUI()
///     {
///         TestPivot comp = target as TestPivot;
///         Pivot.DrawHandles(serializedObject.FindProperty("pivot"), comp.transform);
///     }
/// }
/// #endif
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

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nothke.Utils
{
    [System.Serializable]
    public struct Pivot
    {
        public Vector3 position;
        public Vector3 rotation;

        public void DrawGizmos(Transform relativeTo)
        {
#if UNITY_EDITOR
            Vector3 pos = relativeTo.TransformPoint(position);
            Quaternion rot = relativeTo.rotation * Quaternion.Euler(rotation);

            float size = HandleUtility.GetHandleSize(pos) * 0.3f;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(pos, rot * Vector3.right * size);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(pos, rot * Vector3.forward * size);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(pos, rot * Vector3.up * size);

            Gizmos.DrawSphere(pos, 0.03f);
#endif
        }

#if UNITY_EDITOR
        public static void DrawHandles(SerializedProperty prop, Transform relativeTo)
        {
            const float scaleMult = 0.7f;

            var startMatrix = Handles.matrix;
            Handles.matrix = Matrix4x4.Scale(Vector3.one * scaleMult) * startMatrix;

            var posProp = prop.FindPropertyRelative("position");
            var rotProp = prop.FindPropertyRelative("rotation");

            Vector3 pos = relativeTo.TransformPoint(posProp.vector3Value) / scaleMult;
            Quaternion rot = relativeTo.rotation * Quaternion.Euler(rotProp.vector3Value);

            EditorGUI.BeginChangeCheck();
            Handles.TransformHandle(ref pos, ref rot);

            Handles.matrix = startMatrix;

            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObject(, "Undo pivot change");
                posProp.vector3Value = relativeTo.InverseTransformPoint(pos * scaleMult);
                rotProp.vector3Value = (Quaternion.Inverse(relativeTo.rotation) * rot).eulerAngles;
                prop.serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
