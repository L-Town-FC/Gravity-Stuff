using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{   
    public PlayerDashState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }

    float startTime;
    float dashTime = 0.25f;
    float dashForce = 500f;
    Vector3 dashDir;

    public override void EnterState()
    {
        dashDir = ctx._rb.velocity.normalized;

        if(dashDir == Vector3.zero)
        {
            dashDir = ctx.transform.TransformDirection(Vector3.forward);
        }
        startTime = Time.time;
        ctx._lastDashTime = Time.time;
        ctx._isDash = true;
    }
    public override void UpdateState()
    {
        //Debug.DrawRay(ctx.transform.position, ctx._currentCombinedMoveDir * 5f, Color.blue);

        //adds force in direction that player was initially going when they started the dash
        ctx._rb.AddForce(dashDir * dashForce);
        ctx._rb.maxAngularVelocity = 0f;
        CheckSwitchState();
    }

    public override void FixedUpdateState()
    {


    }

    public override void ExitState()
    {

    }
    public override void InitializeSubState()
    {
    }

    //dash lasts set time instead of distance because using rb.add force makes calculating exact distances every frame difficult
    public override void CheckSwitchState()
    {
        if(Time.time - startTime >= dashTime)
        {
            SwitchState(factory.Idle());
        }
        
    }
}
