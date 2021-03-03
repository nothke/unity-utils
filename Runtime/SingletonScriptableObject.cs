/// SingletonScriptableObject - A self-referencing ScriptableObject, useful for global game settings.
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
