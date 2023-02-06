using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;

public class playerCamera : NetworkBehaviour
{
    private PlayerControls playerControls;
    private InputAction cameraMovement;
    Camera virtualCam;

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

    private void Start()
    {
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }

        //Gives the player a unique camera

        virtualCam = FindObjectOfType<Camera>();
        virtualCam.transform.GetComponent<CinemachineVirtualCamera>().Follow = transform.GetChild(0);
        transform.GetChild(0).position = new Vector3(0, 0.5f, 0);
        

    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        rb.angularVelocity = Vector3.zero;
        cameraInput = new Vector2(cameraMovement.ReadValue<Vector2>().x, cameraMovement.ReadValue<Vector2>().y);
        //Very janky deadzone. will need to fix
        if(Mathf.Abs(cameraInput.x) < 1.5f)
        {
            cameraInput.x = 0f;
        }
        if (Mathf.Abs(cameraInput.y) < 1.5f)
        {
            cameraInput.y = 0f;
        }
        
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        //used to turn player. player is turned instead of camera so player and camera dont go out of synce
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, rb.rotation * Quaternion.Euler(new Vector3(0f, cameraInput.x, 0f) * cameraSensitivity.x * Time.fixedDeltaTime), 0.5f));

        //used to max pan camera up and down. this is camera specific as rotating the players whole body would cause problems
        float currentAngle = playerCam.localEulerAngles.x;

        //unity handles angles weirdly when you go past 360 and this ensures the angle is always between -180f and 180f
        if (currentAngle > 180f)
        {
            currentAngle -= 360f;
        }
        currentAngle -= cameraInput.y * cameraSensitivity.y * Time.fixedDeltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAndMaxCameraAngle.x, minAndMaxCameraAngle.y);
        playerCam.localEulerAngles = new Vector3(currentAngle, 0f, 0f);
    }
}
