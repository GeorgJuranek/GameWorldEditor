using UnityEngine;

public class PlayerRotationer : MonoBehaviour
{
    [SerializeField]
    float rotationHorizontalSpeed = 400f;

    [SerializeField]
    float rotationVerticalSpeed = 200f;

    float currentRotationX;
    float currentRotationY;

    [SerializeField]
    GameObject body;

    [SerializeField]
    Vector2Int minMaxVerticalAngle = new Vector2Int(-10, 15);


    void Update()
    {
        Rotate();
    }

    void Rotate()
    {
        Vector2 move = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        currentRotationX += move.x * rotationHorizontalSpeed * Time.deltaTime;
        currentRotationY += move.y * rotationVerticalSpeed * Time.deltaTime;
        
        currentRotationY = Mathf.Clamp(currentRotationY, minMaxVerticalAngle.x, minMaxVerticalAngle.y);

        Quaternion targetRotationX = Quaternion.Euler(0f, currentRotationX, 0f);
        Quaternion targetRotationY = Quaternion.Euler(-currentRotationY, 0f, 0f);


        transform.localRotation = targetRotationY;

        body.transform.rotation = targetRotationX;

    }
}
