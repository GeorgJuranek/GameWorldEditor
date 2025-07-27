using UnityEngine;
using UnityEngine.AI;
using System;

public class PlayerBobber : MonoBehaviour
{
    [SerializeField]
    NavMeshAgent agent;

    [SerializeField]
    AnimationCurve curve;

    [SerializeField]
    float bobbingSpeed = 1.0f;

    [SerializeField]
    float maximalOffset = 0.1f;

    Vector3 localStartPosition;
    float timer = 0.0f;

    [SerializeField]
    AudioSource audioSource;

    float footstepTimer;
    [SerializeField, Range(0,1)]
    float footStepsRetarder = 0.6f;

    void Start()
    {
        localStartPosition = transform.localPosition;
    }

    void Update()
    {
        if (agent.velocity.magnitude > 1)
        {
            timer += Time.deltaTime * bobbingSpeed;

            float yOffset = curve.Evaluate(timer) * maximalOffset;


            footstepTimer += Time.deltaTime * (agent.velocity.magnitude* footStepsRetarder);
            if (footstepTimer >= 1 && Time.timeScale > 0)
            {
                if (!audioSource.isPlaying && audioSource!=null)
                {
                    audioSource.Play();
                    footstepTimer = 0;
                }
            }

            transform.localPosition = new Vector3(localStartPosition.x, localStartPosition.y + yOffset, localStartPosition.z);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, localStartPosition, Time.deltaTime * 5.0f);
        }
    }
}
