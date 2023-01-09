using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    //TODO: Make base equipment class that so they all have standard naming conventions that can be used

    //state variables
    PlayerBaseState currentState;
    PlayerStateFactory states;

    #region Getters and Setters
    //Components
    public PlayerBaseState _currentState { get { return currentState; } set { currentState = value; } }
    public PlayerStateFactory _states { get { return states; } }
    public Transform _playerCam { get { return playerCam; } set { playerCam = value; } }
    public playerCamera _playerCameraScript { get { return playerCameraScript; } set { playerCameraScript = value; } }
    public Rigidbody _rb { get { return rb; }}

    //Movement
    public Vector3 _movementInput { get { return movementInput; } set { movementInput = value; } }
    public Vector3 _currentMoveInput { get { return currentMoveInput; } set { currentMoveInput = value; } }
    public Vector3 _currentMoveDir { get { return currentMoveDir; } set { currentMoveDir = value; } }
    public Vector3 _currentCombinedMoveDir {  get { return currentCombinedMoveDir; } }
    public float _dirChange { get { return dirChange; } set { dirChange = value; } }
    public bool _isJumpPressed { get { return isJumpPressed; } set { isJumpPressed = value; } }
    public bool _isDashPressed { get { return isDashPressed; } set { isDashPressed = value; } }
    public bool _isGrounded { get { return isGrounded; } set { isGrounded = value; } }

    //Gravity
    public bool _disableGravity { get { return disableGravity; } set { disableGravity = value; } }
    public bool _gravityChange { get { return gravityChange; } set { gravityChange = value; } }
    public bool _checkGravitySwitch { get { return checkGravitySwitch; } set { checkGravitySwitch = value; } }
    public float _verticalVelocity { get { return verticalVelocity; } set { verticalVelocity = value; } }
    public float _accelerationDueToGravity { get { return acceleration; } set { acceleration = value; } }
    public float _gravityChangeCooldownTime { get { return gravityChangeCooldownTime; } }
    public float _lastGravityChangeTime { get { return lastGravityChangeTime; } set { lastGravityChangeTime = value; } }
    public Vector3 _newGravity { get { return newGravity; } set { newGravity = value; } }
    public Vector3 _rotationAxis { get { return rotationAxis; } set { rotationAxis = value; } }
    public Vector3 _up { get { return up; } set { up = value; } }

    //Dash
    public bool _checkDash { get { return checkDash; }  set { checkDash = value; } }
    public bool _isDash { get { return isDash; } set { isDash = value; } }
    public float _dashCooldownTime {  get { return dashCooldownTime; } set { dashCooldownTime = value; } }
    public float _lastDashTime { get { return lastDashTime; } set { lastDashTime = value; } }

    //Equipment
    public bool _checkEquipment {  get { return checkEquipment; } set { checkEquipment = value; } }
    public int _equipmentAmount { get { return equipmentAmount; } set { equipmentAmount = value; } }
    public GameObject _currentEquipment { get { return currentEquipment; } set { currentEquipment = value; } }
    #endregion

    #region Action Maps and Inputs
    //Action maps and inputs
    private PlayerControls playerControls;
    private InputAction movement;
    Vector3 movementInput = Vector3.zero; //holds players inputs
    bool isJumpPressed = false;
    bool isDashPressed = false;
    #endregion

    #region Player Componenets
    CapsuleCollider capsuleCollider;
    Rigidbody rb;
    Transform playerCam;
    playerCamera playerCameraScript;
    #endregion

    #region Gravity Variables
    [SerializeField]
    bool disableGravity = false; //for testing only. disable players gravity
    bool gravityChange = false;
    bool checkGravitySwitch = false;
    float acceleration = -0.75f; //acceleration due to gravity
    public float gravityForce = -1000f;
    public float jumpForce = 1000f;
    float verticalVelocity = 0f; //stores the players vertical velocity due to jumping/gravity
    float gravityChangeCooldownTime = 1.25f;
    float lastGravityChangeTime = 0f;
    Vector3 up;
    Vector3 newGravity;
    Vector3 rotationAxis;
    #endregion

    #region Ground Variables
    //checking if grounded variables
    Vector3[] localLowerBounds; //holds points on player that are raycast from to check if grounded
    bool isGrounded = false;
    #endregion

    #region Movement Variables
    float moveSpeed = 7f; //speed at which players moves
    public float moveForce = 250f;
    public float maxVelocity = 10f;
    float dirChange = 0.25f; //rate at which player can change direction. It takes more time to change direction in the air than on the ground
    Vector3 currentMoveDir = Vector3.zero;
    Vector3 currentMoveInput = Vector3.zero;
    Vector3 currentCombinedMoveDir = Vector3.zero;
    #endregion

    #region Dash Variables
    float dashCooldownTime = 1.25f;
    float lastDashTime = 0f;
    bool checkDash = false;
    bool isDash = false;
    #endregion

    #region Equipment Variables
    bool checkEquipment = false;
    int equipmentAmount = 10;
    [SerializeField]
    int teamNumber;
    [SerializeField]
    GameObject currentEquipment;
    #endregion

    public delegate void GravityChange(float currentTime);
    public static event GravityChange gravityChanged;

    public delegate void DashEvent(float currentTime);
    public static event DashEvent dashed;

    private void Awake()
    {
        states = new PlayerStateFactory(this);
        currentState = states.Idle();
        currentState.EnterState();
        up = transform.up;
        playerCam = transform.GetChild(0);
        playerCameraScript = GetComponent<playerCamera>();
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
        GettingPlayerInputs();
        currentState.UpdateState();
        if(gravityChange)
        {
            gravityChanged(Time.time);
            gravityChange = false;
        }

        if (isDash)
        {
            dashed(Time.time);
            isDash = false;
        }

        if (_isGrounded)
        {
            _rb.drag = 5f;
        }
        else
        {
            _rb.drag = 1f;
        }

        _rb.velocity = maxVelocitySetter(maxVelocity, _rb.velocity, _up);

        Debug.DrawRay(transform.position, transform.TransformDirection(currentMoveDir) * 3f, Color.black);
    }

    //apply inputs to components in fixed update
    private void FixedUpdate()
    {
        //rb.velocity = Vector3.zero; //fixes bug where if you moved into a wall for long enough your velocity would be stuck at a value other than zero

        currentState.FixedUpdateState();

        //currentCombinedMoveDir = transform.up * _verticalVelocity + transform.TransformDirection(_currentMoveDir) * moveSpeed;

        //rb.MovePosition(rb.position + currentCombinedMoveDir * Time.fixedDeltaTime);

        if (_isGrounded)
        {
            rb.AddForce(_up * -10f);
        }
        else
        {
            rb.AddForce(_up * gravityForce);
        }

        rb.AddForce(moveForce * transform.TransformDirection(_currentMoveDir));
    }

    private void OnDisable()
    {
        //need to disable incase object is destroyed and game tries to call them after
        DisablingPlayerControls();
    }


    private void DoJump(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            isJumpPressed = true;
        }
    }

    private void Dash(InputAction.CallbackContext obj)
    {
        if (obj.performed)
        {
            checkDash = true;
        }
    }

    private void UseEquipment(InputAction.CallbackContext obj)
    {
        if (obj.performed)
        {
            _checkEquipment = true;
        }
    }

    public void SpawnEquipment()
    {
        GameObject temp = Instantiate(_currentEquipment, transform.position, Quaternion.identity);
        temp.GetComponent<BubbleShield>().gravityDir = _up;
        temp.transform.up = _up;
        
    }
    private void ChangeGravity(InputAction.CallbackContext obj)
    {
        if (obj.performed)
        {
            checkGravitySwitch = true;
            newGravity = new Vector3(obj.ReadValue<Vector2>().x, 0f, obj.ReadValue<Vector2>().y);
        }
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

    static Vector3 maxVelocitySetter(float maxVelocity, Vector3 currentVelocity, Vector3 currentUP)
    {
        float x = currentVelocity.x;
        float y = currentVelocity.y;
        float z = currentVelocity.z;

        float newX;
        float newY;
        float newZ;

        if(Mathf.Abs(x) > maxVelocity)
        {
            newX = maxVelocity * Mathf.Sign(x);
        }
        else
        {
            newX = x;
        }

        if (Mathf.Abs(y) > maxVelocity)
        {
            newY = maxVelocity * Mathf.Sign(y);
        }
        else
        {
            newY = y;
        }

        if (Mathf.Abs(z) > maxVelocity)
        {
            newZ = maxVelocity * Mathf.Sign(z);
        }
        else
        {
            newZ = z;
        }

        return new Vector3(newX, newY, newZ);
    }

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

        playerControls.PlayerMovement.GravityChange.performed += ChangeGravity;
        playerControls.PlayerMovement.GravityChange.Enable();

        playerControls.PlayerMovement.Jump.performed += DoJump;
        playerControls.PlayerMovement.Jump.Enable();

        playerControls.PlayerMovement.Dash.performed += Dash;
        playerControls.PlayerMovement.Dash.Enable();

        playerControls.PlayerMovement.Equipment.performed += UseEquipment;
        playerControls.PlayerMovement.Equipment.Enable();
    }

    void GettingPlayerInputs()
    {
        //movementInput = new Vector3(movement.ReadValue<Vector2>().x, 0f, movement.ReadValue<Vector2>().y);
        movementInput = new Vector3(Mathf.MoveTowards(currentMoveInput.x, movement.ReadValue<Vector2>().x, _dirChange), 0f, Mathf.MoveTowards(currentMoveInput.z,movement.ReadValue<Vector2>().y, _dirChange));
        _isGrounded = GroundCheck(localLowerBounds, capsuleCollider.radius, -transform.up, transform);
    }

    void DisablingPlayerControls()
    {
        movement.Disable();
        playerControls.PlayerMovement.Jump.Disable();
        playerControls.PlayerMovement.GravityChange.Disable();
        playerControls.PlayerMovement.Dash.Disable();
        playerControls.PlayerMovement.Equipment.Disable();
    }
}
