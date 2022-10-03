using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class baseGun : MonoBehaviour
{
    protected int clipSize;
    protected enum FireType {semi, full}
    protected FireType fireType;
    protected float timeBetweenShots;

    private PlayerControls playerControls;

    Transform endOfBarrel;

    // Start is called before the first frame update

    protected virtual void Awake()
    {
        playerControls = new PlayerControls();
        endOfBarrel = transform.GetChild(0);
    }

    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }
    void Shoot(InputAction.CallbackContext obj)
    {
        print("BANG");
    }

    private void OnEnable()
    {
        playerControls.PlayerMovement.Shoot.performed += Shoot;
        playerControls.PlayerMovement.Shoot.Enable();
    }

    private void OnDisable()
    {
        playerControls.PlayerMovement.Shoot.Disable();
    }
}
