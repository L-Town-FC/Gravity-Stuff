using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    //TODO: Play with accleration rate to make it seem less jarring when you switch gravity
    //TODO: Implement cooldown for gravity changing

    private PlayerControls playerControls;
    private InputAction movement;
    private InputAction cameraMovement;
    Transform playerCam;

    CapsuleCollider capsuleCollider;
    Rigidbody rb;
    Vector3[] localLowerBounds; //holds points on player that are raycast from to check if grounded
    bool isGrounded = false;
    float verticalVelocity; //stores the players vertical velocity due to jumping/gravity
    float acceleration = -1f; //acceleration due to gravity
    float jumpForce = 20f; //velocity magnitude that is set when player jumps
    float moveSpeed = 7f; //speed at which players moves
    Vector3 movementInput = Vector3.zero; //holds players inputs
    Vector2 cameraInput = Vector2.zero; //holds players inputs
    [SerializeField]
    Vector2 cameraSensitivity = new Vector2(0.5f, 0.3f);
    Vector2 minAndMaxCameraAngle = new Vector2(-60f, 60f);
    Vector3 up;

    private void Awake()
    {
        playerControls = new PlayerControls();

        playerCam = transform.GetChild(0);
        capsuleCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        localLowerBounds = GetLowerBounds(capsuleCollider.center, capsuleCollider.radius);
        up = transform.up;
    }

    private void OnEnable()
    {
        //enabling the different player controls
        movement = playerControls.PlayerMovement.Walking;
        movement.Enable();
        cameraMovement = playerControls.PlayerMovement.Camera;
        cameraMovement.Enable();


        playerControls.PlayerMovement.GravityChange.performed += ChangeGravity;
        playerControls.PlayerMovement.GravityChange.Enable();

        playerControls.PlayerMovement.Jump.performed += DoJump;
        playerControls.PlayerMovement.Jump.Enable();
    }
    //grab inputs in update
    private void Update()
    {
        movementInput = new Vector3(movement.ReadValue<Vector2>().x, 0f, movement.ReadValue<Vector2>().y);
        cameraInput = new Vector2(cameraMovement.ReadValue<Vector2>().x, cameraMovement.ReadValue<Vector2>().y);
        isGrounded = GroundCheck(localLowerBounds, capsuleCollider.radius, -transform.up, transform);
        Debug.DrawRay(transform.position, up * 10f);
        Debug.DrawRay(transform.position, transform.forward * 10f, Color.red);

    }

    //apply updates in fixed update
    private void FixedUpdate()
    {
        //SettingNewGravity();
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
    private void OnDisable()
    {
        //need to disable incase object is destroyed and game tries to call them after
        movement.Disable();
        cameraMovement.Disable();
        playerControls.PlayerMovement.Jump.Disable();
        playerControls.PlayerMovement.GravityChange.Disable();
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

    private void ChangeGravity(InputAction.CallbackContext obj)
    {
        if (obj.performed)
        {
            Vector3 newGravity = new Vector3(obj.ReadValue<Vector2>().x, 0f, obj.ReadValue<Vector2>().y);
            if (!(newGravity.x == 0.71f || newGravity.x == -0.71f || newGravity.z == 0.71f || newGravity.z == -0.71f)){
                //transform newGRavity so its relative to player
                //only take largest component of newgravity and set others to zero
                //normalize and set to up
                newGravity = transform.TransformDirection(newGravity);

                Vector3 newUp = new Vector3(Mathf.RoundToInt(newGravity.x), 0f, 0f);
                float max = Mathf.Abs(newGravity.x);

                if(Mathf.Abs(newGravity.y) > max)
                {
                    max = Mathf.Abs(newGravity.y);
                    newUp = new Vector3(0f, Mathf.RoundToInt(newGravity.y), 0f);
                }

                if (Mathf.Abs(newGravity.z) > max)
                {
                    newUp = new Vector3(0f, 0f, Mathf.RoundToInt(newGravity.z));
                }
                up = -newUp;
                StartCoroutine("SettingGravity");
            }
        }
        //checks the direction the player pressed on the d-pad and compares it world vectors and then sets the players up values equal to world vector that is closest to the input direction
    }


    void CamMove()
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

    IEnumerator SettingGravity()
    {
        bool isFinished = false;
        Vector3 finalDirection = transform.position + transform.TransformDirection(Vector3.forward);
        while (!isFinished)
        {
            if(Vector3.Dot(transform.up, up) >= 0.99999f)
            {
                transform.rotation.SetLookRotation(finalDirection, up);
                Vector3 fineAdjustment = transform.localEulerAngles;
                float x = Mathf.RoundToInt(fineAdjustment.x);
                float y = Mathf.RoundToInt(fineAdjustment.y);
                float z = Mathf.RoundToInt(fineAdjustment.z);
                transform.localEulerAngles = new Vector3(x, y, z);
                print(transform.localEulerAngles);
                isFinished = true;
            }
            Quaternion slopeRotation = Quaternion.FromToRotation(transform.up, up);

            transform.rotation = Quaternion.Lerp(transform.rotation, slopeRotation * transform.rotation, 15f * Time.deltaTime);

            yield return null;
        }
    }

}
