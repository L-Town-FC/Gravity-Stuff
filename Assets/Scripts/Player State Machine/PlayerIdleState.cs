using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) 
    :base (currentContext, playerStateFactory){ }

    public override void EnterState()
    {
        Debug.Log("Idle");
    }
    public override void UpdateState()
    {
        CheckSwitchState();
    }
    public override void ExitState()
    {
    }
    public override void InitializeSubState()
    {
    }
    public override void CheckSwitchState()
    {
        //if player tries to switch gravity and all checks pass
        if (ctx.IsGravitySwitched)
        {
            SwitchState(factory.SwitchGravity());
        }
    }
}
