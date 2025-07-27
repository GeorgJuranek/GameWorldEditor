using UnityEngine;

public class PlayerInstance : MonoBehaviour
{
    public static PlayerInstance Instance;

    private void Awake()
    {
        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
