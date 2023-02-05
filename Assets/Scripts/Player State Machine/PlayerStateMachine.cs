using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerStateMachine : NetworkBehaviour
{
    //state variables
    PlayerBaseState currentState;
    PlayerStateFactory states;
    [SerializeField]
    GameObject PlayerUI;

    #region Getters and Setters
    //Components
    public PlayerBaseState _currentState { get { return currentState; } set { currentState = value; } }
    public PlayerStateFactory _states { get { return states; } }
    public Transform _playerCam { get { return playerCam; } set { playerCam = value; } }
    public playerCamera _playerCameraScript { get { return playerCameraScript; } set { playerCameraScript = value; } }
    public Rigidbody _rb { get { return rb; }}
    public CapsuleCollider _capsuleCollider { get { return capsuleCollider; } }

    //Movement
    public Vector3 _movementInput { get { return movementInput; } set { movementInput = value; } }
    public Vector3 _currentMoveInput { get { return currentMoveInput; } set { currentMoveInput = value; } }
    public Vector3 _currentMoveDir { get { return currentMoveDir; } set { currentMoveDir = value; } }
    public bool _isGrounded { get { return isGrounded; } set { isGrounded = value; } }

    //Gravity
    public bool _gravityChange { get { return gravityChange; } set { gravityChange = value; } }
    public float _gravityChangeCooldownTime { get { return gravityChangeCooldownTime; } }
    public float _lastGravityChangeTime { get { return lastGravityChangeTime; } set { lastGravityChangeTime = value; } }
    public float _jumpForce { get { return jumpForce; } set { jumpForce = value; } }
    public float _gravityForce { get { return gravityForce; } }
    public Vector3 _rotationAxis { get { return rotationAxis; } set { rotationAxis = value; } }
    public Vector3 _up { get { return up; } set { up = value; } }

    //Dash
    public bool _isDash { get { return isDash; }  set { isDash = value; } }
    public float _dashCooldownTime {  get { return dashCooldownTime; } set { dashCooldownTime = value; } }
    public float _lastDashTime { get { return lastDashTime; } set { lastDashTime = value; } }

    //Equipment
    public int _equipmentAmount { get { return equipmentAmount; } set { equipmentAmount = value; } }
    public GameObject _currentEquipment { get { return currentEquipment; } set { currentEquipment = value; } }
    #endregion

    #region Action Maps and Inputs
    private PlayerControls playerControls;
    private InputAction movement;
    Vector3 movementInput = Vector3.zero; //holds players inputs
    #endregion

    #region Player Componenets
    CapsuleCollider capsuleCollider;
    Rigidbody rb;
    Transform playerCam;
    playerCamera playerCameraScript;
    #endregion

    #region Gravity Variables
    [SerializeField]
    bool gravityChange = false;
    float gravityForce = -30f;
    float fallingGravityMultiplier = 2f;
    float jumpForce = 60f;
    float gravityChangeCooldownTime = 1.25f;
    float lastGravityChangeTime = 0f;
    Vector3 up;
    Vector3 rotationAxis;
    #endregion

    #region Ground Variables
    //checking if grounded variables
    Vector3[] localLowerBounds; //holds points on player that are raycast from to check if grounded
    bool isGrounded = false;
    #endregion

    #region Movement Variables
    public float moveForce = 200f;
    public float maxNonVerticalVelocity = 6f;
    public float maxVerticalVelocity = 18f;
    float airSpeedReduction = 0.5f;
    Vector3 currentMoveDir = Vector3.zero;
    Vector3 currentMoveInput = Vector3.zero;
    #endregion

    #region Dash Variables
    float dashCooldownTime = 1.25f;
    float lastDashTime = 0f;
    bool isDash = false;
    #endregion

    #region Equipment Variables
    [SerializeField]
    int equipmentAmount = 100;
    [SerializeField]
    int teamNumber;
    [SerializeField]
    GameObject currentEquipment;
    #endregion

    private void Awake()
    {
        states = new PlayerStateFactory(this);
        currentState = states.Idle();
        currentState.EnterState();
        up = transform.up;
        playerCam = transform.GetChild(0);
        playerCameraScript = GetComponent<playerCamera>();

        //Creates UI for Player. Will probably move into separate function
        GameObject temp = Instantiate(PlayerUI);
        temp.GetComponent<PlayerUI>().player = transform.gameObject;
        temp.SetActive(true);

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
        if (!IsOwner)
        {
            return;
        }
        GettingPlayerInputs();
        currentState.UpdateState();

        //player has more control on the ground than in the air
        if (_isGrounded)
        {
            _rb.drag = 5f;
        }
        else
        {
            _rb.drag = 0.75f;
        }

        _rb.velocity = maxVelocitySetter(maxNonVerticalVelocity, maxVerticalVelocity, _rb.velocity, _up);

        Debug.DrawRay(transform.position, transform.TransformDirection(currentMoveDir) * 3f, Color.black);
    }

    //apply inputs to components in fixed update
    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        currentState.FixedUpdateState();

        if (_isGrounded)
        {
            //rb.AddForce(_up * -10f);
        }
        else
        {
            //checks if the player is currently falling by comparing their velocity with their current up
            //if they aligned or equal they are not falling
            //if they are falling then increase gravity to make them seem less floaty
            if (Vector3.Dot(rb.velocity, _up) < 0f)
            {
                rb.AddForce(_up * gravityForce * fallingGravityMultiplier);
            }
            else
            {
                rb.AddForce(_up * gravityForce);
            }
        }

        rb.AddForce(moveForce * transform.TransformDirection(_currentMoveDir));

    }


    private void OnDisable()
    {
        //need to disable incase object is destroyed and game tries to call them after
        DisablingPlayerControls();
    }

    #region Input Actions

    #endregion

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

    #region Velocity Functions
    //these functions are used to check and set the players max velocities so they cant infinitly gain speed

    //if falling velocity is the same as moving velocity, the player either feels slow and floaty or uncontrollably fast
    //this function checks which direction is the falling direction and clamps its speed to a larger range of values than the horizontal velocities
    static Vector3 maxVelocitySetter(float maxNonVerticalVelocity, float maxVerticalVelocity, Vector3 currentVelocity, Vector3 currentUP)
    {
        float x = currentVelocity.x;
        float y = currentVelocity.y;
        float z = currentVelocity.z;

        x = fallDirChecker(currentUP, Vector3.right, x, maxNonVerticalVelocity, maxVerticalVelocity);
        y = fallDirChecker(currentUP, Vector3.up, y, maxNonVerticalVelocity, maxVerticalVelocity);
        z = fallDirChecker(currentUP, Vector3.forward, z, maxNonVerticalVelocity, maxVerticalVelocity);

        return new Vector3(x,y,z);
    }

    //uses the dot product to compare the current up with a unit vector
    //if the dot product is zero, i.e. the unit vector is not aligned with the current up, its clamped with the maxNonVerticalVelocity
    //if the dot prodcut is 1 then the current up is parallel with the unit vector meaning we are looking at the "up" component of the players velocity and it is clamped using the maxverticalvelocity value
    static float fallDirChecker(Vector3 currentUp, Vector3 velocityVector, float currentVelocity, float maxNonVerticalVelocity, float maxVerticalVelocity)
    {
        if(Mathf.Abs(Vector3.Dot(currentUp, velocityVector)) > 0f)
        {
            currentVelocity = Mathf.Clamp(currentVelocity, -maxVerticalVelocity, maxVerticalVelocity);
        }
        else
        {
            currentVelocity = Mathf.Clamp(currentVelocity, -maxNonVerticalVelocity, maxNonVerticalVelocity);
            //tired of rb.velocity being 10^-8 and not zero
            if (Mathf.Abs(currentVelocity) - 0.0001f < 0)
            {
                currentVelocity = 0;
            }
        }

        return currentVelocity;
    }

    #endregion

    #region Initializing Player and Getting Inputs
    void SettingInitialPlayerConditions()
    {
        playerControls = new PlayerControls();
        capsuleCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        localLowerBounds = GetLowerBounds(capsuleCollider.center, capsuleCollider.radius);
    }

    void SettingPlayerControls()
    {
        movement = playerControls.PlayerMovement.Walking;
        movement.Enable();
    }

    void GettingPlayerInputs()
    {
        movementInput = new Vector3(movement.ReadValue<Vector2>().x, 0f, movement.ReadValue<Vector2>().y);
        _isGrounded = GroundCheck(localLowerBounds, capsuleCollider.radius, -transform.up, transform);
        if (!_isGrounded)
        {
            movementInput *= airSpeedReduction;
        }
    }

    void DisablingPlayerControls()
    {
        movement.Disable();
    }
    #endregion
}
