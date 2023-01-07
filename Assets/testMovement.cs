using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class testMovement : MonoBehaviour
{
    Rigidbody rb;

    private PlayerControls playerControls;
    private InputAction movement;
    Vector3 movementInput = Vector3.zero; //holds players inputs
    CapsuleCollider capsuleCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerControls = new PlayerControls();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        movementInput = new Vector3(movement.ReadValue<Vector2>().x, 0f, movement.ReadValue<Vector2>().y);
    }

    private void FixedUpdate()
    {
        rb.AddForce(movementInput * 2f, ForceMode.VelocityChange);
    }

    private void OnEnable()
    {
        movement = playerControls.PlayerMovement.Walking;
        movement.Enable();
    }

    private void OnDisable()
    {
        movement.Disable();
    }
}
