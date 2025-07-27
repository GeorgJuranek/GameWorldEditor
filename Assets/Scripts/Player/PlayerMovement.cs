using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    float minimumSpeed = 5f;

    [SerializeField]
    float speed = 5f;

    NavMeshAgent agent;

    float velocityInput;

    [SerializeField]
    float velocityDeceleration = 0.75f;

    [SerializeField]
    float maxBackwardsVelocity = -3, maxForwardsVelocity = 10;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        float moveVertical = Input.mouseScrollDelta.y;

        velocityInput += (moveVertical * speed) * Time.deltaTime;
        velocityInput = Mathf.Clamp(velocityInput, maxBackwardsVelocity, maxForwardsVelocity);

        agent.speed = Mathf.Max(minimumSpeed, minimumSpeed + Mathf.Abs(velocityInput));

        Vector3 movement = transform.forward * velocityInput;

        if (velocityInput > 0)
        {
            velocityInput -= velocityDeceleration * Time.deltaTime;
        }
        else if (velocityInput < 0)
        {
            velocityInput += velocityDeceleration * Time.deltaTime;
        }

        if (Mathf.Abs(velocityInput) > 0.01f)
        {
            //NavMeshPath navMeshPath = new NavMeshPath();

            Vector3 target = transform.position + movement;
            //if (agent.CalculatePath(target, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
            //{
            agent.SetDestination(target);
            //}
        }
    }
}
