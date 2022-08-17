using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    CharacterController charController;
    CapsuleCollider capsuleCollider;

    Vector3 moveDir = Vector2.zero;
    [SerializeField]
    float moveSpeed = 7f;
    Vector3 localDown;
    float verticalVelocity;
    bool isGrounded;
    [SerializeField]
    float acceleration = 1f;

    [SerializeField]
    Vector3[] localLowerBounds;

    // Start is called before the first frame update
    void Start()
    {
        charController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        localDown = -transform.up;
        localLowerBounds = GetLowerBounds(capsuleCollider.center, capsuleCollider.radius);
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Vector3 bound in localLowerBounds)
        {
            Debug.DrawRay(transform.TransformPoint(bound), localDown * 3f, Color.red);
        }

        isGrounded = GroundCheck(localLowerBounds, capsuleCollider.radius, charController.skinWidth, localDown, transform);
    }

    private void FixedUpdate()
    {
        if (!isGrounded)
        {
             verticalVelocity = Gravity(verticalVelocity, acceleration);
        }
        else
        {
            verticalVelocity = Mathf.Clamp(verticalVelocity, Mathf.NegativeInfinity, 0.1f);
            
        }

        charController.Move(localDown * verticalVelocity * Time.fixedDeltaTime);
        charController.Move(moveDir * moveSpeed * Time.fixedDeltaTime);
    }

    public void PlayerMovement(InputAction.CallbackContext context)
    {
        Vector2 rawInput = context.ReadValue<Vector2>();

        moveDir = new Vector3(rawInput.x, 0f, rawInput.y);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && GroundCheck(localLowerBounds, capsuleCollider.radius, charController.skinWidth, localDown, transform))
        {
            verticalVelocity = -20f;
        }
    }

    static float Gravity(float verticalVelocity, float acceleration)
    {
        verticalVelocity += acceleration;
        return verticalVelocity;
    }

    static bool GroundCheck(Vector3[] lowerBounds, float radius, float skinWidth, Vector3 down, Transform transform)
    {
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
