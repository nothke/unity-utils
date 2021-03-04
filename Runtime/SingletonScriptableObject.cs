///
/// SingletonScriptableObject by Nothke
/// 
/// A self-referencing ScriptableObject, useful for global game settings.
/// 
/// Example usage:
/// 
/// 1. Create a script like:
/// 
/// [CreateAssetMenu(menuName = "MySingletonSO", fileName = "MySingletonSO")]
/// public class MySingletonSO : SingletonScriptableObject<MySingletonSO>
/// {
///     public float myFloat;
/// }
/// 
/// 2. Create an asset in the Resources folder (IMPORTANT that it should be in Resources!!!)
/// 3. Now you can get the data from anywhere by using MySingletonSO.Instance.myFloat;
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

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    static T _instance = null;
    public static T Instance
    {
        get
        {
            if (!_instance)
                FindAndInit();

            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void FindAndInit()
    {
        var objs = Resources.FindObjectsOfTypeAll<T>();

        if (objs.Length == 0)
        {
            Resources.LoadAll("");
            objs = Resources.FindObjectsOfTypeAll<T>();
        }

#if UNITY_EDITOR
        // The resource might not be loaded by the editor on start, so force loading all assets first.
        // Might be perf problem if you have a lot of assets!

        if (objs.Length == 0)
            Debug.LogError("A singleton instance of " + typeof(T).ToString() + " not found.");
#endif

        _instance = objs[0];
        (_instance as SingletonScriptableObject<T>).Initialize();
    }

    protected virtual void Initialize() { }
}
