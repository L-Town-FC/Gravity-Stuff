using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class learningEvents : MonoBehaviour
{
    //need to add collision detection
    private PlayerControls playerControls;
    private InputAction movement;
    private InputAction cameraMovement;

    CapsuleCollider capsuleCollider;
    Rigidbody rb;
    Vector3[] localLowerBounds;
    bool isGrounded = false;
    float verticalVelocity;
    float acceleration = -1f; //acceleration due to gravity
    float jumpForce = 20f;
    float moveSpeed = 7f;
    Vector3 movementInput = Vector3.zero;
    Vector2 cameraInput = Vector2.zero;
    [SerializeField]
    Vector2 cameraSensitivity = new Vector2(0.5f, 100f);

    private void Awake()
    {
        playerControls = new PlayerControls();

        capsuleCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        localLowerBounds = GetLowerBounds(capsuleCollider.center, capsuleCollider.radius);
    }

    private void OnEnable()
    {
        movement = playerControls.PlayerMovement.Walking;
        movement.Enable();
        cameraMovement = playerControls.PlayerMovement.Camera;
        cameraMovement.Enable();

        playerControls.PlayerMovement.Jump.performed += DoJump;
        playerControls.PlayerMovement.Jump.Enable();
    }
    //grab inputs in update
    private void Update()
    {
        movementInput = new Vector3(movement.ReadValue<Vector2>().x, 0f, movement.ReadValue<Vector2>().y);
        cameraInput = new Vector2(cameraMovement.ReadValue<Vector2>().x, cameraMovement.ReadValue<Vector2>().y);
        isGrounded = GroundCheck(localLowerBounds, capsuleCollider.radius, -transform.up, transform);

        
    }

    //apply updates in fixed update
    private void FixedUpdate()
    {
        if (!isGrounded) //if the player is grounded reset their downward motion so it doesnt constantly build up. if they are not grounded, begin applying gravity
        {
            verticalVelocity = Gravity(verticalVelocity, acceleration);
        }
        else
        {
            verticalVelocity = Mathf.Clamp(verticalVelocity, 0f, Mathf.Infinity); //lowerlimit is -0.1f to make sure it always reached ground and doesnt hover slightly above the ground, positive infinity is so a jump force can be added
        }

        rb.MovePosition(rb.position + transform.up * verticalVelocity * Time.fixedDeltaTime + transform.TransformDirection(movementInput * moveSpeed * Time.fixedDeltaTime));
        CamMove();
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

    private void OnDisable()
    {
        //need to disable incase object is destroyed and game tries to call them after
        movement.Disable();
        cameraMovement.Disable();
        playerControls.PlayerMovement.Jump.Disable();
    }

    void CamMove()
    {
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, rb.rotation * Quaternion.Euler(new Vector3(0f, cameraInput.x, 0f) * cameraSensitivity.x * Time.fixedDeltaTime), 0.5f));
    }

    static float Gravity(float verticalVelocity, float acceleration) //simple way to add gravity to player calculations
    {
        verticalVelocity += acceleration;
        if(verticalVelocity <= -30f)
        {
            verticalVelocity = -30f;
        }
        return verticalVelocity;
    }

    static Vector3[] GetLowerBounds(Vector3 center, float radius)
    {
        //go through points on lower sphere of capsule (front, back, left, right and center) and write them to array for later ground detection
        Vector3 newCenter = new Vector3(center.x, center.y - radius, center.z);
        Vector3 frontBound = new Vector3(newCenter.x, newCenter.y, newCenter.z + radius);
        Vector3 rearBound = new Vector3(newCenter.x, newCenter.y, newCenter.z - radius);
        Vector3 leftBound = new Vector3(newCenter.x - radius, newCenter.y, newCenter.z);
        Vector3 rightBound = new Vector3(newCenter.x + radius, newCenter.y, newCenter.z);
        Vector3 bottom = center;

        Vector3[] lowerBounds = new Vector3[5];
        lowerBounds[0] = frontBound;
        lowerBounds[1] = rearBound;
        lowerBounds[2] = leftBound;
        lowerBounds[3] = rightBound;
        lowerBounds[4] = bottom;

        return lowerBounds;
    }

    static bool GroundCheck(Vector3[] lowerBounds, float radius, Vector3 down, Transform transform)
    {
        foreach (Vector3 bound in lowerBounds) //draws rays from 5 points on players body straight down for visualization purposes
        {
            Debug.DrawRay(transform.TransformPoint(bound), -transform.up * 3f, Color.red);
        }
        //go through array of bounds and cast down slightly more than distance from the point to the ground when perfectly grounded. if any of the raycasts hit, then we now that we are grounded
        //radius is radius of the lower sphere that makes up the capsule, skinWidth is a buffer that is set by the characterController to make sure the player doesnt clip into other objects
        foreach (Vector3 bound in lowerBounds)
        {
            if (Physics.Raycast(transform.TransformPoint(bound), down, radius * 1.01f))
            {
                return true;
            }
        }
        return false;
    }
}
