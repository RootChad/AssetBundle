using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;   // Speed of movement
    public float lookSpeed = 2f;    // Speed of mouse look

    private float yaw = 0f;
    private float pitch = 0f;

    void Update()
    {
        // Keyboard movement
        float moveForward = Input.GetKey(KeyCode.W) ? 1 : (Input.GetKey(KeyCode.S) ? -1 : 0);
        float moveRight = Input.GetKey(KeyCode.D) ? 1 : (Input.GetKey(KeyCode.A) ? -1 : 0);
        float moveUp = Input.GetKey(KeyCode.E) ? 1 : (Input.GetKey(KeyCode.Q) ? -1 : 0);

        Vector3 moveDirection = new Vector3(moveRight, moveUp, moveForward);
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Mouse look
        yaw += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");

        pitch = Mathf.Clamp(pitch, -90f, 90f);  // Limit vertical rotation

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }
}