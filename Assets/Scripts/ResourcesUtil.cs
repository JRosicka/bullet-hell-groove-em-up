using UnityEditor;
using UnityEngine;

public static class ResourcesUtil {
    public static T[] GetAllScriptableObjectInstances<T>() where T : ScriptableObject {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new []{"Assets/Resources"});
        T[] a = new T[guids.Length];
        for (int i = 0; i < guids.Length; i++) {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }

        return a;
    }
}