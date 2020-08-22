using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float CameraSpeed = 1.0f;
    public float VerticalSpeedModifier = 1.0f;
    public float HorizontalSpeedModifier = 1.0f;
    //public float MaxZoomDistance = 70.0f;
    //public float MinZoomDistance = 15.0f;
    //public float MouseSensitivity = 1800.0f;
    //public float ZoomSensitivity = 200.0f;
    public float PanSensitivity = 60.0f;

    private float DefaultCameraDistance;

    private Camera CameraComponent;

    // TODO:
    //      - Add zoom
    //      - Add rotation
    //      - Add edge scrolling
    //      - False collision with world bounds

    private void Start()
    {
        CameraComponent = this.gameObject.GetComponent<Camera>();

        Cursor.visible = false;
        DefaultCameraDistance = Vector3.Distance(transform.position, CameraComponent.transform.position);
    }

    void Update()
    {
        // Move the camera using the player's input axes
        float verticalMove = Input.GetAxis("Vertical") * CameraSpeed * VerticalSpeedModifier;
        float horizontalMove = Input.GetAxis("Horizontal") * CameraSpeed * HorizontalSpeedModifier;

        // Now perform the actual camera movement
        Vector3 desiredMove = CameraComponent.transform.up * verticalMove + CameraComponent.transform.right * horizontalMove;
        transform.Translate(desiredMove * PanSensitivity * Time.deltaTime);

        // Zoom the camera in and out using mouse scroll delta
        float desiredZoomMagnitude = Input.mouseScrollDelta.y;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Cursor.visible = !Cursor.visible;
        }
    }
}
