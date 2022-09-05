using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerCamera : MonoBehaviour
{
    private PlayerControls playerControls;
    private InputAction cameraMovement;

    Rigidbody rb;
    Transform playerCam;
    Vector2 cameraInput = Vector2.zero; //holds players inputs
                                        //camera variables
    [SerializeField]
    Vector2 cameraSensitivity = new Vector2(0.5f, 0.3f);
    public static Vector2 minAndMaxCameraAngle = new Vector2(-75f, 75f);

    private void Awake()
    {
        playerControls = new PlayerControls();

        rb = GetComponent<Rigidbody>();
        playerCam = transform.GetChild(0);
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        cameraMovement = playerControls.PlayerMovement.Camera;
        cameraMovement.Enable();
    }

    private void OnDisable()
    {
        cameraMovement.Disable();
    }

    private void Update()
    {
        cameraInput = new Vector2(cameraMovement.ReadValue<Vector2>().x, cameraMovement.ReadValue<Vector2>().y);
    }

    private void FixedUpdate()
    {
        //used to turn player. player is turned instead of camera so player and camera dont go out of synce
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, rb.rotation * Quaternion.Euler(new Vector3(0f, cameraInput.x, 0f) * cameraSensitivity.x * Time.fixedDeltaTime), 0.5f));

        //used to max pan camera up and down. this is camera specific as rotating the players whole body would cause problems
        float currentAngle = playerCam.localEulerAngles.x;

        if (currentAngle > 180f)
        {
            currentAngle -= 360f;
        }
        currentAngle -= cameraInput.y * cameraSensitivity.y * Time.fixedDeltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAndMaxCameraAngle.x, minAndMaxCameraAngle.y);
        playerCam.localEulerAngles = new Vector3(currentAngle, 0f, 0f);
    }
}
