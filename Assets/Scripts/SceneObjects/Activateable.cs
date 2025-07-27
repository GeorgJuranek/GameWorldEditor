using UnityEngine;
using UnityEngine.Events;

public class Activateable : MonoBehaviour
{
    private void OnEnable()
    {
        Disable();
    }

    [SerializeField]
    UnityEvent objectEnable;

    [SerializeField]
    UnityEvent objectDisable;

    public void Enable()
    {
        objectEnable?.Invoke();
    }

    public void Disable()
    {
        objectDisable?.Invoke();
    }
}
