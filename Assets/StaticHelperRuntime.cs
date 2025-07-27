using UnityEngine;

public static class StaticHelperRuntime
{
    public static GameObject InstantiateAtRuntime(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (Application.isPlaying)
        {
            GameObject newInstance = Object.Instantiate(prefab, position, rotation);
            return newInstance;
        }

        Debug.LogError("This method is only meant to be used during runtime (Play Mode).");
        return null;
    }

    public static GameObject InstantiateAtRuntime(GameObject prefab)
    {
        if (Application.isPlaying)
        {
            GameObject newInstance = Object.Instantiate(prefab);
            return newInstance;
        }

        Debug.LogError("This method is only meant to be used during runtime (Play Mode).");
        return null;
    }
}
