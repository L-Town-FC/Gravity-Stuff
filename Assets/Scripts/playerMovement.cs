using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovement : MonoBehaviour
{
    //TODO: fix up/down on d-pad with fix gravity

    public bool dontStop;

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
    public static float gravityChangeCooldownTime = 0.5f;
    float lastGravityChangeTime = 0f;
    float acceleration = -0.75f; //acceleration due to gravity
    public bool disableGravity = false;

    Vector3[] localLowerBounds; //holds points on player that are raycast from to check if grounded
    bool isGrounded = false;
    float verticalVelocity = 0f; //stores the players vertical velocity due to jumping/gravity
    float jumpForce = 20f; //velocity magnitude that is set when player jumps
    float moveSpeed = 7f; //speed at which players moves

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
        }
        else
        {
            verticalVelocity = Mathf.Clamp(verticalVelocity, 0f, Mathf.Infinity); //lowerlimit is -0.1f to make sure it always reached ground and doesnt hover slightly above the ground, positive infinity is so a jump force can be added
        }

        if (disableGravity)
        {
            verticalVelocity = 0f;
        }
        

        rb.MovePosition(rb.position + transform.up * verticalVelocity * Time.fixedDeltaTime + transform.TransformDirection(movementInput * moveSpeed * Time.fixedDeltaTime));
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
            if(Time.time < lastGravityChangeTime + gravityChangeCooldownTime) //checks when the player last changed their gravity and blocks them from changing again until cooldown is done
            {
                return;
            }

            //checks the direction the player pressed on the d-pad and compares it world vectors and then sets the players up values equal to world vector that is closest to the input direction
            newGravity = transform.TransformDirection(newGravity);

            Vector3 newUp = new Vector3(Mathf.RoundToInt(newGravity.x), 0f, 0f);
            float max = Mathf.Abs(newGravity.x);

            if (Mathf.Abs(newGravity.y) >= max)
            {
                max = Mathf.Abs(newGravity.y);
                newUp = new Vector3(0f, Mathf.RoundToInt(newGravity.y), 0f);
            }

            if (Mathf.Abs(newGravity.z) >= max)
            {
                newUp = new Vector3(0f, 0f, Mathf.RoundToInt(newGravity.z));
            }

            up = -newUp;

            lastGravityChangeTime = Time.time;

            if(gravityChanged != null) //calling event to alert other scripts that the gravity has successfully changed
            {
                gravityChanged(lastGravityChangeTime);
            }

            Vector3 rotationAxis;

            newGravity = new Vector3(- obj.ReadValue<Vector2>().y, 0f, obj.ReadValue<Vector2>().x);
            rotationAxis = newGravity;
            print(up);
            StartCoroutine("NewSettingGravity", rotationAxis);
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
        movementInput = new Vector3(movement.ReadValue<Vector2>().x, 0f, movement.ReadValue<Vector2>().y);
        isGrounded = GroundCheck(localLowerBounds, capsuleCollider.radius, -transform.up, transform);
    }

    void DisablingPlayerControls()
    {
        movement.Disable();
        playerControls.PlayerMovement.Jump.Disable();
        playerControls.PlayerMovement.GravityChange.Disable();
    }

    IEnumerator NewSettingGravity(Vector3 _rotationAxis)
    {
        playerCameraScript.enabled = false; //disables players ability to move their camera around during gravity flip. this ensures that player cant screw up coroutine by moving during it
        disableGravity = true;
        Vector2 cameraLimits = playerCamera.minAndMaxCameraAngle;
        //changes the players up direction to its newly set one

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

        transform.Rotate(_rotationAxis, 90, Space.World);

        Vector3 playerLookDirection = playerLookPoint - playerCam.position;
        Vector3 projectedLookAngle = Vector3.ProjectOnPlane(playerLookDirection, up);
        float bodyAngleBetween = Vector3.SignedAngle(transform.forward, projectedLookAngle, up);
        transform.Rotate(Vector3.up, bodyAngleBetween, Space.Self);


        //DOESNT WORK YET
        //Moving playerCam up and down is the only thing left
        //DO THAT HERE
        playerLookDirection = playerLookPoint - playerCam.position;
        Vector3 playerCamRotationAxis = playerCam.right;
        float playerCamToFromAngle = Vector3.SignedAngle(playerCam.forward, playerLookDirection, playerCamRotationAxis);


        transform.rotation = origRotation;
        playerCam.rotation = camRoration;

        int counter = 0;
        int maxIterations = 90;
        float camAngleIncrement = playerCamToFromAngle / (float)maxIterations;
        while(counter < maxIterations)
        {
            Debug.DrawLine(playerCam.position, playerLookPoint, Color.green);
            Debug.DrawRay(playerCam.position, playerCam.forward * 20f, Color.red);
            transform.Rotate(_rotationAxis * 90 / maxIterations, Space.World);
            transform.Rotate(Vector3.up * bodyAngleBetween / (float)maxIterations, Space.Self);
            playerCam.Rotate(Vector3.right * playerCamToFromAngle / (float)maxIterations, Space.Self);
 
            counter++;
            yield return null;
        }


        while (dontStop)
        {

            Debug.DrawRay(transform.position, transform.right * 5f, Color.red);
            yield return null;
        }

        playerCameraScript.enabled = true;
        //disableGravity = false;
        StopCoroutine("NewSettingGravity");

        yield return null;
    }

    IEnumerator SettingGravity() //may need to disable players camera inputs during this
    {
        playerCameraScript.enabled = false; //disables players ability to move their camera around during gravity flip. this ensures that player cant screw up coroutine by moving during it

        Debug.DrawRay(transform.position, up * 5f, Color.cyan);
        //changes the players up direction to its newly set one
        bool isFinished = false;
        Vector3 finalDirection = transform.position + transform.TransformDirection(Vector3.forward);

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

        //projects the vector from what the player is looking at onto new up facing plane so the players body can be rotated towards it during gravity change
        //this ensures that the player is facing the same direction before and after the change in gravity
        Vector3 projectedBodyLook = Vector3.ProjectOnPlane((playerLookPoint - playerCam.position).normalized, up);

        int counter = 0;
        while (!isFinished)
        {
            counter++;
            float dot = Vector3.Dot(transform.up, up);
            float camDot = Vector3.Dot(playerCam.forward, (playerLookPoint - playerCam.position).normalized);

            Quaternion slopeRotation = Quaternion.FromToRotation(transform.up, up);

            transform.rotation = Quaternion.Lerp(transform.rotation, slopeRotation * transform.rotation, 7f * Time.deltaTime);

            if(dot <= 0.98f)
            {
                float bodyToLookPointAngle = Vector3.SignedAngle(transform.forward, projectedBodyLook, up); //rotates the body towards the point it was looking at slowly during its flip
                if(Mathf.Abs(bodyToLookPointAngle) > 1f)
                {
                    transform.Rotate(transform.InverseTransformVector(up), Mathf.Sign(bodyToLookPointAngle) * 0.8f);
                }
            }

            if(camDot <= 0.995f)
            {
                float angleBetween = Vector3.Angle(transform.up, playerCam.forward) - Vector3.Angle(transform.up, playerLookPoint - playerCam.position);
                playerCam.localEulerAngles += new Vector3(-Mathf.Sign(angleBetween) * 0.25f, 0f, 0f);
                print(angleBetween);
            }


            if (dot >= 0.99999f) //checks if the players current up is close to its target up
            {
                float angleSensitivity = 3f;
                //whens its close it just manually sets it to be exact
                transform.rotation.SetLookRotation(finalDirection, up);

                //rounds the players euler angles to intergers. without this the player may be tilted by a few degrees which would be jarring/look bad
                Vector3 fineAdjustment = transform.localEulerAngles;

                //lets nearest interger be angleSensitivity away instead of 1. player was occasionally getting stuck anywhere between desired angle (90,180,270,360) +/- angleSensitivity degrees because of the lack of precision of dot product
                float x = Mathf.RoundToInt(fineAdjustment.x / angleSensitivity) * angleSensitivity;
                float y = Mathf.RoundToInt(fineAdjustment.y / angleSensitivity) * angleSensitivity;
                float z = Mathf.RoundToInt(fineAdjustment.z / angleSensitivity) * angleSensitivity;


                transform.localEulerAngles = new Vector3(x, y, z);
                playerCameraScript.enabled = true; //re-enables players ability to look around once flip is complete
                isFinished = true;
            }

            yield return null;
        }

        StopCoroutine("SettingGravity");
    }

}
