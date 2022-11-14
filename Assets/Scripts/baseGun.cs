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
    protected float recoilPerShot = 0.5f;
    [SerializeField]
    protected GameObject bullet;

    Animator anim;

    private PlayerControls playerControls;

    Transform endOfBarrel;
    Transform playerCam;

    public delegate void UpdateAmmo(int currentAmmo, int maxAmmo);
    public static event UpdateAmmo ammoUpdate;

    // Start is called before the first frame update

    protected virtual void Awake()
    {
        playerControls = new PlayerControls();
        anim = GetComponent<Animator>();
        //janky way of getting playerCam
        playerCam = transform.parent.parent;
        endOfBarrel = transform.GetChild(0);
        bulletsRemaining = clipSize;
    }

    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //Shooting();
        if(bulletsRemaining <= 0)
        {
            anim.SetBool("isShooting", false);
        }
        Debug.DrawRay(endOfBarrel.position, endOfBarrel.forward * 5f, Color.black);
    }
    //checks when trigger is pulled
    void Shoot(InputAction.CallbackContext obj)
    {
        if (obj.performed)
        {
            isShooting = true;
            anim.SetBool("isShooting", true);
        }

        if (obj.canceled)
        {
            isShooting = false;
            anim.SetBool("isShooting", false);
        }
    }
    //checks if reload button has been pushed
    void Reload(InputAction.CallbackContext obj)
    {
        if (obj.performed && bulletsRemaining != clipSize)
        {
            anim.SetBool("isReloading", true);
        }
    }

    public void ReloadAnimation()
    {
        bulletsRemaining = clipSize;
        if (ammoUpdate != null)
        {
            ammoUpdate(bulletsRemaining, clipSize);
        }
        anim.SetBool("isReloading", false);

    }

    //determines when gun actually shoots
    //void Shooting()
    //{
    //    //player shouldnt be allowed to shoot if they are performing another action
    //    if(playerStateManager.currentPlayerState != playerStateManager.PlayerState._default)
    //    {
    //        return;
    //    }

    //    //player wont shoot if they arent holding down trigger
    //    if (!isShooting)
    //    {
    //        return;
    //    }

    //    //player wont shoot with an empty clip
    //    if(bulletsRemaining == 0)
    //    {
    //        return;
    //    }

    //    if (Time.time - timeOfLastShot > timeBetweenShots)
    //    {
    //        SpawnBullet();
    //        AddRecoil();
    //    }

    //    //disables shooting after one trigger pull so the player needs to release and re-press shoot button
    //    //leave option open for burst fire weapons
    //    if (fireType == FireType.semi)
    //    {
    //        isShooting = false;
    //    }// continues to fire as long as trigger is held
    //    else if (fireType == FireType.fullAuto)
    //    {

    //    }
    //}

    //spawns a bullet at the end of the barrel and fires it
    void SpawnBullet()
    {
        timeOfLastShot = Time.time;
        GameObject temp = Instantiate(bullet, endOfBarrel.position, Quaternion.identity);

        //adjusts the bullets trajectory so it always hits what the crosshair is looking at. This needs to be done because the player forward isnt aligned with the guns forward
        Vector3 newForward;
        RaycastHit hit;

        if(Physics.Raycast(new Ray(playerCam.position, playerCam.forward), out hit, 50f))
        {
            newForward = hit.point - endOfBarrel.position;
        }
        else
        {
            newForward = playerCam.forward * 50f + playerCam.position - endOfBarrel.position;
        }

        temp.transform.forward = newForward;
        bulletsRemaining -= 1;
        if(ammoUpdate != null)
        {
            ammoUpdate(bulletsRemaining, clipSize);
        }
    }

    void AddRecoil()
    {
        playerCam.localEulerAngles -= new Vector3(recoilPerShot, 0f, 0f);
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
