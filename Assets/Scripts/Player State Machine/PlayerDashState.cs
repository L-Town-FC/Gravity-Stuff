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
        dashDir = ctx.transform.TransformDirection(ctx._currentMoveDir + new Vector3(0f, ctx._verticalVelocity/30f, 0f));
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
    public override void CheckSwitchState()
    {
        if(Time.time - startTime >= dashTime)
        {
            SwitchState(factory.Idle());
        }
        
    }
}
