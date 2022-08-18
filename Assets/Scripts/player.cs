using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    //outside components and transforms
    CharacterController charController;
    CapsuleCollider capsuleCollider;
    Transform playerCam;

    //movement parameters
    [SerializeField]
    float moveSpeed = 7f;
    Vector3 moveDir = Vector2.zero;

    //camera movement parameters
    Vector2 cameraMovement = Vector2.zero;
    Vector2 minAndMaxCameraAngle = new Vector2(-60f, 60f);
    Vector2 cameraMoveSpeed = new Vector2(250f, 220f);

    //gravity and jumping parameteres
    float verticalVelocity;
    bool isGrounded;
    [SerializeField]
    float acceleration = -1f; //acceleration due to gravity
    float jumpForce = 20f;

    [SerializeField]
    Vector3[] localLowerBounds;

    //negative is up because of how -transform.up is always positive

    // Start is called before the first frame update
    void Start()
    {
        charController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        playerCam = transform.GetChild(0);
        Cursor.lockState = CursorLockMode.Locked;
        localLowerBounds = GetLowerBounds(capsuleCollider.center, capsuleCollider.radius);
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Vector3 bound in localLowerBounds) //draws rays from 5 points on players body straight down for visualization purposes
        {
            Debug.DrawRay(transform.TransformPoint(bound), -transform.up * 3f, Color.red);
        }

        isGrounded = GroundCheck(localLowerBounds, capsuleCollider.radius, charController.skinWidth, -transform.up, transform);

        transform.eulerAngles += Vector3.up * cameraMovement.x * cameraMoveSpeed.x * Time.deltaTime; //lets the player turn left and right

        //takes the current up/down camera angle and adds however much the mouse moves up and down multiplied by a set move speed
        float camAngle = playerCam.eulerAngles.x;
        if(camAngle > 180f) //arbitrary number above max camera angle
        {
            camAngle -= 360f;
        }
        camAngle += -cameraMovement.y * cameraMoveSpeed.y * Time.deltaTime;
        camAngle = Mathf.Clamp(camAngle, minAndMaxCameraAngle.x, minAndMaxCameraAngle.y);
        playerCam.localEulerAngles = new Vector3(camAngle, 0f, playerCam.eulerAngles.z);
        

    }

    private void FixedUpdate()
    {
        if (!isGrounded) //if the player is grounded reset their downward motion so it doesnt constantly build up. if they are not grounded, begin applying gravity
        {
             verticalVelocity = Gravity(verticalVelocity, acceleration);
        }
        else
        {
            verticalVelocity = Mathf.Clamp(verticalVelocity, -0.1f, Mathf.Infinity); //lowerlimit is -0.1f to make sure it always reached ground and doesnt hover slightly above the ground, positive infinity is so a jump force can be added
            
        }

        charController.Move(transform.up * verticalVelocity * Time.fixedDeltaTime); //this is where gravity and jump velocity is applied to the body
        charController.Move(transform.TransformDirection(moveDir) * moveSpeed * Time.fixedDeltaTime);
    }

    public void PlayerMovement(InputAction.CallbackContext context)
    {
        Vector2 rawInput = context.ReadValue<Vector2>();

        moveDir = new Vector3(rawInput.x, 0f, rawInput.y);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        //lets the player jump by setting the players upward velocity if they are currently grounded. this ground check is a security blanket in case the one in update misses do to frame differences
        if (context.performed && GroundCheck(localLowerBounds, capsuleCollider.radius, charController.skinWidth, -transform.up, transform))
        {
            verticalVelocity = jumpForce;
        }
    }

    public void CameraControls(InputAction.CallbackContext context)
    {
        cameraMovement = context.ReadValue<Vector2>().normalized;
    }
    static float Gravity(float verticalVelocity, float acceleration) //simple way to add gravity to player calculations
    {
        verticalVelocity += acceleration;
        return verticalVelocity;
    }

    static bool GroundCheck(Vector3[] lowerBounds, float radius, float skinWidth, Vector3 down, Transform transform)
    {
        //go through array of bounds and cast down slightly more than distance from the point to the ground when perfectly grounded. if any of the raycasts hit, then we now that we are grounded
        //radius is radius of the lower sphere that makes up the capsule, skinWidth is a buffer that is set by the characterController to make sure the player doesnt clip into other objects
        foreach(Vector3 bound in lowerBounds)
        {
            if(Physics.Raycast(transform.TransformPoint(bound), down, radius + skinWidth))
            {
                return true;
            }
        }
        return false;
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
}
