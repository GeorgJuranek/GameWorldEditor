using UnityEngine;

public class RotationRandomizer : MonoBehaviour
{
    void Start()
    {
        RandomizeRotationAroundY();
    }

    void RandomizeRotationAroundY()
    {
        transform.Rotate(Vector3.up, Random.Range(0, 361));
    }
}
