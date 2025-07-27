using UnityEditor;
using UnityEngine;

public static class StaticHelper
{
    public static GameObject InstantiateAsPrefabInEditor(GameObject gameObject, Vector3 position, Quaternion rotation)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            GameObject newPrefabInstance = PrefabUtility.InstantiatePrefab(gameObject) as GameObject;
            newPrefabInstance.transform.position = position;
            newPrefabInstance.transform.rotation = rotation;

            return newPrefabInstance;
        }
#endif
        Debug.LogError("Can't use method outside of Editor!");
        return null;
    }

    public static GameObject InstantiateAsPrefabInEditor(GameObject gameObject)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            GameObject newPrefabInstance = PrefabUtility.InstantiatePrefab(gameObject) as GameObject;
            return newPrefabInstance;
        }
#endif
        Debug.LogError("Can't use method outside of Editor!");
        return null;
    }
}
