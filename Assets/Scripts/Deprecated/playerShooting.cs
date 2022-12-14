using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerShooting : MonoBehaviour
{
    private PlayerControls playerControls;
    Transform gunHolder;
    [SerializeField]
    GameObject gun;
    Transform playerCam;
    GameObject playerGun;
    Transform endOfBarrel;

    private void Awake()
    {
        playerControls = new PlayerControls();
        playerCam = transform.GetChild(0);
        gunHolder = playerCam.GetChild(0);
        playerGun = Instantiate(gun, gunHolder.position, Quaternion.identity, gunHolder);
        endOfBarrel = playerGun.transform.GetChild(0);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerGun.transform.rotation = playerCam.transform.rotation;
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
