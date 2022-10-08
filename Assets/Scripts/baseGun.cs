using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class baseGun : MonoBehaviour
{
    protected int clipSize;
    protected enum FireType {semi, fullAuto}
    [SerializeField]
    protected FireType fireType;
    protected bool isShooting = false;
    [SerializeField]
    protected float timeBetweenShots = 1f;
    protected float timeOfLastShot = -5f;
    [SerializeField]
    protected GameObject bullet;

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
        Shooting();
        Debug.DrawRay(endOfBarrel.position, endOfBarrel.forward * 5f, Color.black);
    }
    //checks when trigger is pulled
    void Shoot(InputAction.CallbackContext obj)
    {
        if (obj.performed)
        {
            isShooting = true;
        }

        if (obj.canceled)
        {
            isShooting = false;
        }
    }

    //determines when gun actually shoots
    void Shooting()
    {
        if (!isShooting)
        {
            return;
        }
        if(fireType == FireType.semi)
        {
            if (Time.time - timeOfLastShot > timeBetweenShots)
            {
                timeOfLastShot = Time.time;
                isShooting = false;
                
                GameObject temp = Instantiate(bullet, endOfBarrel.position, Quaternion.identity);
                temp.transform.forward = endOfBarrel.forward;
            }
        }
        else if(fireType == FireType.fullAuto)
        {
            if (Time.time - timeOfLastShot > timeBetweenShots)
            {
                timeOfLastShot = Time.time;
                GameObject temp = Instantiate(bullet, endOfBarrel.position, Quaternion.identity);
                temp.transform.forward = endOfBarrel.forward;
            }
        }
    }

    private void OnEnable()
    {
        playerControls.PlayerMovement.Shoot.performed += Shoot;
        playerControls.PlayerMovement.Shoot.canceled += Shoot;
        playerControls.PlayerMovement.Shoot.Enable();
    }

    private void OnDisable()
    {
        playerControls.PlayerMovement.Shoot.Disable();
    }
}
