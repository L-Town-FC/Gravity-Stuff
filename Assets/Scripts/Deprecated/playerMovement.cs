using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovement : MonoBehaviour
{
    //TODO: Move changingGravity into player action manager so everything can access it

    //Action maps and inputs
    private PlayerControls playerControls;
    private InputAction movement;
    Vector3 movementInput = Vector3.zero; //holds players inputs

    //player components
    CapsuleCollider capsuleCollider;
    Rigidbody rb;
    Transform playerCam;
    playerCamera playerCameraScript;

    //gravity variables
    public static Vector3 up;
    public static float gravityChangeCooldownTime = 0.75f;
    float lastGravityChangeTime = 0f;
    float acceleration = -0.75f; //acceleration due to gravity
    [SerializeField]
    bool disableGravity = false; //for testing only. disable players gravity
    public bool changingGravity = false;
    [SerializeField]
    float gravityChangeSpeed = 10f; //rate at which players gravity is changed. higher number means faster
    float verticalVelocity = 0f; //stores the players vertical velocity due to jumping/gravity
    float jumpForce = 20f; //velocity magnitude that is set when player jumps

    //checking if grounded variables
    Vector3[] localLowerBounds; //holds points on player that are raycast from to check if grounded
    bool isGrounded = false;

    //movement variables
    float moveSpeed = 7f; //speed at which players moves
    Vector3 currentMoveDir = Vector3.zero;
    Vector3 currentMoveInput = Vector3.zero;
    Vector3 currentCombinedMoveDir = Vector3.zero;

    //rate at which player can change direction. It takes more time to change direction in the air than on the ground
    float maxGroundDirChange = 0.25f;
    float maxAirDirChange = 0.1f;
    float dirChange = 0.25f;

    public delegate void GravityChange(float currentTime);
    public static event GravityChange gravityChanged;

    private void Awake()
    {
        SettingInitialPlayerConditions();
    }

    private void OnEnable()
    {
        //enabling the different player controls
        SettingPlayerControls();
    }
    //grab inputs in update
    private void Update()
    {
        GettingPlayerInputs();
    }

    //apply inputs to components in fixed update
    private void FixedUpdate()
    {
        rb.velocity = Vector3.zero; //fixes bug where if you moved into a wall for long enough your velocity would be stuck at a value other than zero

        if (!isGrounded) //if the player is grounded reset their downward motion so it doesnt constantly build up. if they are not grounded, begin applying gravity
        {
            verticalVelocity = Gravity(verticalVelocity, acceleration);
            dirChange = maxAirDirChange;
        }
        else
        {
            verticalVelocity = Mathf.Clamp(verticalVelocity, 0f, Mathf.Infinity); //lowerlimit is -0.1f to make sure it always reached ground and doesnt hover slightly above the ground, positive infinity is so a jump force can be added
            dirChange = maxGroundDirChange;
        }

        if (disableGravity)
        {
            verticalVelocity = 0f;
        }

        //sets players direction to last input direction. This will be used to let the player continue in the direction they were moving even when the when their is no input
        //this will be used to handle ramp down speed when moving and also potentially sliding if I feel like implementing that
        if(movementInput.magnitude != 0f)
        {
            currentMoveDir = movementInput;
        }
        else
        {
            currentMoveDir = Vector3.zero;
        }

        //may change in future, player now continues direction they were last moving in before gravity change, while gravity change is taking place. Seems off right now
        if (!changingGravity)
        {
            currentCombinedMoveDir = transform.up * verticalVelocity + transform.TransformDirection(currentMoveDir) * moveSpeed;
        }
        rb.MovePosition(rb.position + currentCombinedMoveDir * Time.fixedDeltaTime);

        currentMoveInput = movementInput;
    }

    private void OnDisable()
    {
        //need to disable incase object is destroyed and game tries to call them after
        DisablingPlayerControls();
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
            Vector3 newGravity = new Vector3(obj.ReadValue<Vector2>().x, 0f, obj.ReadValue<Vector2>().y); //grabbing inputs

            if ((newGravity.x == 0.71f || newGravity.x == -0.71f || newGravity.z == 0.71f || newGravity.z == -0.71f)){ //this makes sure that the player didnt hit d-pad diagonally. only up, down, left, or right
                return;
            }
            if(Time.time < lastGravityChangeTime + gravityChangeCooldownTime || playerStateManager.currentPlayerState != playerStateManager.PlayerState._default) //checks when the player last changed their gravity and blocks them from changing again until cooldown is done
            {
                return;
            }

            //checks the direction the player pressed on the d-pad and compares it world vectors and then sets the players up values equal to world vector that is closest to the input direction
            newGravity = transform.TransformDirection(newGravity);

            Vector3 newUp = new Vector3(newGravity.x, 0f, 0f);
            float max = Mathf.Abs(newGravity.x);

            if (Mathf.Abs(newGravity.y) >= max)
            {
                max = Mathf.Abs(newGravity.y);
                newUp = new Vector3(0f, newGravity.y, 0f);
            }

            if (Mathf.Abs(newGravity.z) >= max)
            {
                newUp = new Vector3(0f, 0f, newGravity.z);
            }

            up = -newUp;

            lastGravityChangeTime = Time.time;


            Vector3 rotationAxis = transform.TransformDirection(new Vector3(-obj.ReadValue<Vector2>().y, 0f, obj.ReadValue<Vector2>().x));
            Vector3 finalRotationAxis = Vector3.zero;

            max = -0.01f;
            if(Mathf.Abs(rotationAxis.x) >= max)
            {
                max = Mathf.Abs(rotationAxis.x);
                finalRotationAxis = new Vector3(Mathf.Sign(rotationAxis.x), 0f, 0f);
            }

            if (Mathf.Abs(rotationAxis.y) >= max)
            {
                max = Mathf.Abs(rotationAxis.y);
                finalRotationAxis = new Vector3(0f, rotationAxis.y, 0f);
            }

            if (Mathf.Abs(rotationAxis.z) >= max)
            {
                finalRotationAxis = new Vector3(0f, 0f,rotationAxis.z);
            }

            if(gravityChanged != null) //calling event to alert other scripts that the gravity has successfully changed
            {
                gravityChanged(lastGravityChangeTime);
            }

            StartCoroutine("SettingGravity", finalRotationAxis);
        }
        
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

    void SettingInitialPlayerConditions()
    {
        playerControls = new PlayerControls();

        capsuleCollider = GetComponent<CapsuleCollider>();
        playerCameraScript = GetComponent<playerCamera>();
        playerCam = transform.GetChild(0);
        rb = GetComponent<Rigidbody>();
        localLowerBounds = GetLowerBounds(capsuleCollider.center, capsuleCollider.radius);
        up = transform.up;
    }

    void SettingPlayerControls()
    {
        movement = playerControls.PlayerMovement.Walking;
        movement.Enable();

        playerControls.PlayerMovement.GravityChange.performed += ChangeGravity;
        playerControls.PlayerMovement.GravityChange.Enable();

        playerControls.PlayerMovement.Jump.performed += DoJump;
        playerControls.PlayerMovement.Jump.Enable();
    }

    void GettingPlayerInputs()
    {
        //movementInput = new Vector3(movement.ReadValue<Vector2>().x, 0f, movement.ReadValue<Vector2>().y);
        movementInput = new Vector3(Mathf.MoveTowards(currentMoveInput.x, movement.ReadValue<Vector2>().x, dirChange), 0f, Mathf.MoveTowards(currentMoveInput.z,movement.ReadValue<Vector2>().y, dirChange));

        isGrounded = GroundCheck(localLowerBounds, capsuleCollider.radius, -transform.up, transform);
    }

    void DisablingPlayerControls()
    {
        movement.Disable();
        playerControls.PlayerMovement.Jump.Disable();
        playerControls.PlayerMovement.GravityChange.Disable();
    }

    IEnumerator SettingGravity(Vector3 _rotationAxis)
    {
        //Purpose: This IEnumerator's purpose is to alter which direction gravity is for the player. To do this, a new gravity direction is chosen by some player means
        //Gravity and player controls are disabled to make the players movement more predictable when this IEnumerator is triggered
        //The player is rotated so that their players up is opposite the new gravity direction
        //This IEnumerator also attempts to make the player look at the same spot throughout the transition to make the transition less jarring
        changingGravity = true;
        playerCameraScript.enabled = false; //disables players ability to move their camera around during gravity flip. this ensures that player cant screw up coroutine by moving during it
        disableGravity = true;
        //playerStateManager.currentPlayerState = playerStateManager.PlayerState.flipping; //sets globals player state to flipping gravity so the player cant do other actions while flipping
        playerStateManager.playerStatesQueue.Enqueue(playerStateManager.PlayerState.flipping);

        //trying to make it so player is looking at same spot after rotation as before
        //grabs the location of the point that the player is looking at. if the point is significantly far away, a point a set distance away is used instead
        Vector3 playerLookPoint;
        RaycastHit hit;
        if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, 60f))
        {
            playerLookPoint = hit.point;
        }
        else
        {
            playerLookPoint = playerCam.position + playerCam.forward * 60f;
        }

        //need to precalculate this value and then precalculate how much the player needs to rotate to look at target
        Quaternion origRotation = transform.rotation;
        Quaternion camRoration = playerCam.rotation;

        transform.Rotate(_rotationAxis, 90f, Space.World);

        Vector3 playerLookDirection = playerLookPoint - playerCam.position;
        Vector3 projectedLookAngle = Vector3.ProjectOnPlane(playerLookDirection, up);
        float bodyAngleBetween = Vector3.SignedAngle(transform.forward, projectedLookAngle, up);
        transform.Rotate(Vector3.up, bodyAngleBetween, Space.Self);

        //stops the player from completely rotating around in order to look at object when flipping gravity back or forward
        //may disable this
        if(bodyAngleBetween > 150f)
        {
            bodyAngleBetween -= 180f;
        }else if(bodyAngleBetween < -150f)
        {
            bodyAngleBetween += 180f;
        }

        //calculates angle between playerCam and previous look position
        playerLookDirection = playerLookPoint - playerCam.position;
        Vector3 playerCamRotationAxis = playerCam.right;
        float playerCamToFromAngle = Vector3.SignedAngle(playerCam.forward, playerLookDirection, playerCamRotationAxis);

        transform.rotation = origRotation;
        playerCam.rotation = camRoration;

        float incrementor = 0f; //holds how many degrees player has turned already
        float increment = gravityChangeSpeed * Time.fixedDeltaTime;
        float maxChange = 90f; //degrees players axis will change by

        //the players yaw and cameras pitch rates are based on the players axis turning rate.The faster the turning rate, the larger the increment, and the faster everything else occurs
        while (incrementor < maxChange)
        {
            if (incrementor + increment > 90f)
            {
                increment = maxChange - incrementor;
            }
            Debug.DrawLine(playerCam.position, playerLookPoint, Color.green);
            Debug.DrawRay(playerCam.position, playerCam.forward * 20f, Color.red);
            transform.Rotate(_rotationAxis * increment, Space.World); //aligns transform.up with new calculated up
            transform.Rotate(Vector3.up * bodyAngleBetween * increment / maxChange, Space.Self); //rotates player body locally to face towards previous looked at spot
            playerCam.Rotate(Vector3.right * playerCamToFromAngle * increment / maxChange, Space.Self); //adjusts camera to look at previously looked at spot

            incrementor += increment;
            yield return null;
        }

        //because quaternions make no sense this needed to be added because the player would regularly be slightly off alignment with a cardinal direction
        //if not perfectly aligned with different cardinal unit vectors the rotation doesnt complete fully. this is a workaround that checks the remaining angle and completes it
        //is barely noticable but something to look at when bored one day
        float remainingAngle = Vector3.Angle(transform.up, up);
        int count = 15;
        for (int i = 0; i < count; i++)
        {
            transform.Rotate(_rotationAxis, remainingAngle / count, Space.World);
            yield return null;
        }

        //final adujustment because quaternions are not to be trusted
        //may need to switch this to truncation as rounding to int may be too jarring
        float x = Mathf.Round(transform.localEulerAngles.x);
        float y = Mathf.Round(transform.localEulerAngles.y);
        float z = Mathf.Round(transform.localEulerAngles.z);

        transform.localEulerAngles = new Vector3(x, y, z);

        playerCameraScript.enabled = true;
        disableGravity = false;
        changingGravity = false;
        //playerStateManager.currentPlayerState = playerStateManager.PlayerState._default; //sets globals player state to flipping gravity so the player cant do other actions while flipping
        playerStateManager.playerStatesQueue.Enqueue(playerStateManager.PlayerState._default);

        StopCoroutine("SettingGravity");

        yield return null;
    }
}
