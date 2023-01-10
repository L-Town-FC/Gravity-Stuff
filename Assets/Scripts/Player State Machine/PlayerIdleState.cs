using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerIdleState : PlayerBaseState
{
    //TODO: Move input action stuff into this script from the state machine script so I dont have to deal with all these different variables
    //May be harder than I thought

    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) 
    :base (currentContext, playerStateFactory){ }

    //MAY NEED TO MOVE THIS VALUE INTO PLAYER STATE MACHINE SCRIPT SO UI CAN ACCESS IT
    bool switchGravity = false; //set to true if conditions are met to flip players gravity
    bool dash = false;

    public override void EnterState()
    {
    }
    public override void UpdateState()
    {
        if (ctx._checkEquipment)
        {
            if(ctx._equipmentAmount > 0)
            {
                ctx.SpawnEquipment();
                ctx._equipmentAmount -= 1;
            }
            ctx._checkEquipment = false;
        }
        if (ctx._checkGravitySwitch)
        {
            ChangeGravityCheck(ctx._newGravity);
            ctx._checkGravitySwitch = false;
        }

        if (ctx._checkDash)
        {
            DashCheck();
            ctx._checkDash = false;
        }

        CheckSwitchState();
    }

    public override void FixedUpdateState()
    {
        if (ctx._isJumpPressed)
        {
            //ctx._verticalVelocity = jumpForce;
            ctx._rb.AddForce(ctx.jumpForce * ctx._up, ForceMode.Impulse);
            ctx._isJumpPressed = false;
        }

        if (ctx._movementInput.magnitude != 0f)
        {
            ctx._currentMoveDir = ctx._movementInput;
        }
        else
        {
            ctx._currentMoveDir = Vector3.zero;
        }

        ctx._currentMoveInput = ctx._movementInput;
    }

    public override void ExitState()
    {

    }
    public override void InitializeSubState()
    {
    }
    public override void CheckSwitchState()
    {
        if(switchGravity)
        {
            SwitchState(factory.SwitchGravity());
            switchGravity = false;
        }else if (dash)
        {
            SwitchState(factory.Dash());
            dash = false;
        }

    }

    void ChangeGravityCheck(Vector3 obj)
    {
        //Vector3 newGravity = new Vector3(obj.x, 0f, obj.z); //grabbing inputs
        Vector3 newGravity = ctx._newGravity;

        if ((newGravity.x == 0.71f || newGravity.x == -0.71f || newGravity.z == 0.71f || newGravity.z == -0.71f))
        { //this makes sure that the player didnt hit d-pad diagonally. only up, down, left, or right
            return;
        }
        if (Time.time < ctx._lastGravityChangeTime + ctx._gravityChangeCooldownTime) //checks when the player last changed their gravity and blocks them from changing again until cooldown is done
        {
            return;
        }

        //checks the direction the player pressed on the d-pad and compares it world vectors and then sets the players up values equal to world vector that is closest to the input direction
        newGravity = ctx.transform.TransformDirection(newGravity);

        Vector3 newUp = new Vector3(newGravity.x, 0f, 0f);
        float max = Mathf.Abs(newGravity.x);

        if (Mathf.Abs(newGravity.y) >= max)
        {
            max = Mathf.Abs(newGravity.y);
            newUp = new Vector3(0f, newGravity.y, 0f);
        }

        if (Mathf.Abs(newGravity.z) >= max)
        {
            newUp = new Vector3(0f, 0f, newGravity.z);
        }

        ctx._up = -newUp;

        ctx._lastGravityChangeTime = Time.time;


        Vector3 rotationAxis = ctx.transform.TransformDirection(new Vector3(-obj.z, 0f, obj.x));
        Vector3 finalRotationAxis = Vector3.zero;

        //makes sure final rotation angle is perfectly lined up with one of the 8 cardinal directions
        max = -0.01f;
        if (Mathf.Abs(rotationAxis.x) >= max)
        {
            max = Mathf.Abs(rotationAxis.x);
            finalRotationAxis = new Vector3(Mathf.Sign(rotationAxis.x), 0f, 0f);
        }

        if (Mathf.Abs(rotationAxis.y) >= max)
        {
            max = Mathf.Abs(rotationAxis.y);
            finalRotationAxis = new Vector3(0f, rotationAxis.y, 0f);
        }

        if (Mathf.Abs(rotationAxis.z) >= max)
        {
            finalRotationAxis = new Vector3(0f, 0f, rotationAxis.z);
        }

        ctx._gravityChange = true;

        ctx._rotationAxis = finalRotationAxis;

        switchGravity = true;
    }

    void DashCheck()
    {
        if(Time.time - ctx._lastDashTime >= ctx._dashCooldownTime)
        {
            dash = true;
            
        }
    }

}
