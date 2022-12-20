using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) 
    :base (currentContext, playerStateFactory){ }
    float jumpForce = 20f;

    //rate at which player can change direction. It takes more time to change direction in the air than on the ground
    float maxGroundDirChange = 0.25f;
    float maxAirDirChange = 0.1f;
    public override void EnterState()
    {
        Debug.Log("Idle");
    }
    public override void UpdateState()
    {
        CheckSwitchState();
    }

    public override void FixedUpdateState()
    {
        if (ctx._isJumpPressed)
        {
            ctx._verticalVelocity = jumpForce;
            ctx._isJumpPressed = false;
        }

        if (!ctx._isGrounded)
        {
            ctx._verticalVelocity = Gravity(ctx._verticalVelocity, ctx._accelerationDueToGravity);
            ctx._dirChange = maxAirDirChange;
        }
        else
        {
            ctx._verticalVelocity = Mathf.Clamp(ctx._verticalVelocity, 0f, Mathf.Infinity); //lowerlimit is -0.1f to make sure it always reached ground and doesnt hover slightly above the ground, positive infinity is so a jump force can be added
            ctx._dirChange = maxGroundDirChange;
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
       
    }

    static float Gravity(float verticalVelocity, float acceleration) //simple way to add gravity to player calculations
    {
        verticalVelocity += acceleration;
        if (verticalVelocity <= -30f)
        {
            verticalVelocity = -30f;
        }
        return verticalVelocity;
    }

}
