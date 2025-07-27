using UnityEngine;

public class TargetRotationer : MonoBehaviour
{
    Transform Target { get; set; }

    private void Start()
    {
        if (PlayerInstance.Instance != null)
            Target = PlayerInstance.Instance.gameObject.transform;
    }

    void FixedUpdate()
    {
        if (PlayerInstance.Instance != null)
        {
            if (Vector3.Distance(transform.position, Target.position)<1000f)
            {
                if (Quaternion.Angle(transform.rotation, Target.rotation)> 3)
                    transform.LookAt(Target);
            }
        }
    }
}
