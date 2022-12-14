using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class baseGun : MonoBehaviour
{
    //TODO: Make it so shooting is disabled during other actions
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

    AudioSource audioSource;
    [SerializeField]
    AudioClip reloadSound;
    [SerializeField]
    AudioClip shootSound;

    private PlayerControls playerControls;
    PlayerStateMachine playerStateMachine;

    Transform endOfBarrel;
    Transform playerCam;

    public delegate void UpdateAmmo(int currentAmmo, int maxAmmo);
    public static event UpdateAmmo ammoUpdate;

    LayerMask layerMask = new LayerMask();

    // Start is called before the first frame update

    protected virtual void Awake()
    {
        playerControls = new PlayerControls();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        //janky way of getting playerCam
        playerCam = transform.parent.parent;

        //janky way of getting playerState machine
        playerStateMachine = playerCam.parent.GetComponent<PlayerStateMachine>();

        endOfBarrel = transform.GetChild(0);
        bulletsRemaining = clipSize;

        layerMask = LayerMask.GetMask("Default");
    }

    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //simplest way i could think of stopping shooting while flipping gravity. will probably need to be changed later
        //the player can only shoot in the idle state right now. Design choice to make it so there isn't too much going on at once
        if(playerStateMachine._currentState.ToString() != playerStateMachine._states.Idle().ToString())
        {
            anim.enabled = false;
        }
        else
        {
            anim.enabled = true;
        }

        //if the gun has no ammo remaining, it shouldnt be allowed to fire until its been reloaded
        if(bulletsRemaining <= 0)
        {
            anim.SetBool("isShooting", false);
        }
        //Debug.DrawRay(endOfBarrel.position, endOfBarrel.forward * 5f, Color.black);
    }
    //checks when trigger is pulled
    void Shoot(InputAction.CallbackContext obj)
    {
        //player can shoot when the trigger is pushed down
        if (obj.performed)
        {
            isShooting = true;
            anim.SetBool("isShooting", true);
        }

        //player cant shoot when the trigger isnt pushed down
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
        //lets the player reload
        bulletsRemaining = clipSize;
        if (ammoUpdate != null)
        {
            ammoUpdate(bulletsRemaining, clipSize);
        }
        anim.SetBool("isReloading", false);

    }

    void ReloadSound()
    {
        audioSource.Stop();
        audioSource.pitch = 2f;
        audioSource.clip = reloadSound;
        audioSource.Play();
    }

    //spawns a bullet at the end of the barrel and fires it
    void SpawnBullet()
    {
        timeOfLastShot = Time.time;
        GameObject temp = Instantiate(bullet, endOfBarrel.position, Quaternion.identity);

        //adjusts the bullets trajectory so it always hits what the crosshair is looking at. This needs to be done because the player forward isnt aligned with the guns forward
        Vector3 newForward;
        RaycastHit hit;

        if(Physics.Raycast(new Ray(playerCam.position, playerCam.forward), out hit, 50f, layerMask))
        {
            newForward = hit.point - endOfBarrel.position;
        }
        else
        {
            newForward = playerCam.forward * 50f + playerCam.position - endOfBarrel.position;
        }

        Debug.DrawRay(transform.position, newForward * 5f);

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

    void ShootingSound()
    {
        //https://sfxr.me/ good site
        audioSource.clip = shootSound;
        audioSource.pitch = 0.25f;
        audioSource.Play();
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
