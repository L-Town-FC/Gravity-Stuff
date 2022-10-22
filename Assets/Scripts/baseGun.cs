using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class baseGun : MonoBehaviour
{
    protected int clipSize = 30;
    protected int bulletsRemaining;
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

    public delegate void UpdateAmmo(int currentAmmo, int maxAmmo);
    public static event UpdateAmmo ammoUpdate;

    // Start is called before the first frame update

    protected virtual void Awake()
    {
        playerControls = new PlayerControls();
        endOfBarrel = transform.GetChild(0);
        bulletsRemaining = clipSize;
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
    void Reload(InputAction.CallbackContext obj)
    {
        if (obj.performed)
        {
            bulletsRemaining = clipSize;
            if (ammoUpdate != null)
            {
                ammoUpdate(bulletsRemaining, clipSize);
            }
        }
    }

    //determines when gun actually shoots
    void Shooting()
    {
        if (!isShooting)
        {
            return;
        }

        if(bulletsRemaining == 0)
        {
            return;
        }

        if(fireType == FireType.semi)
        {
            if (Time.time - timeOfLastShot > timeBetweenShots)
            {
                SpawnBullet();
                isShooting = false;
            }
        }
        else if(fireType == FireType.fullAuto)
        {
            if (Time.time - timeOfLastShot > timeBetweenShots)
            {
                SpawnBullet();
            }
        }
    }


    void SpawnBullet()
    {
        timeOfLastShot = Time.time;
        GameObject temp = Instantiate(bullet, endOfBarrel.position, Quaternion.identity);
        temp.transform.forward = endOfBarrel.forward;
        bulletsRemaining -= 1;
        if(ammoUpdate != null)
        {
            ammoUpdate(bulletsRemaining, clipSize);
        }
    }

    private void OnEnable()
    {
        playerControls.PlayerMovement.Shoot.performed += Shoot;
        playerControls.PlayerMovement.Shoot.canceled += Shoot;
        playerControls.PlayerMovement.Reload.performed += Reload;
        playerControls.PlayerMovement.Shoot.Enable();
        playerControls.PlayerMovement.Reload.Enable();


    }

    private void OnDisable()
    {
        playerControls.PlayerMovement.Shoot.performed -= Shoot;
        playerControls.PlayerMovement.Shoot.canceled -= Shoot;
        playerControls.PlayerMovement.Reload.performed -= Reload;
        playerControls.PlayerMovement.Shoot.Disable();
        playerControls.PlayerMovement.Reload.Enable();
    }
}
