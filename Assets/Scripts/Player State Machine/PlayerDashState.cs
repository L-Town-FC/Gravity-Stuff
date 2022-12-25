using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{   
    public PlayerDashState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        Debug.Log("Dash");
    }
    public override void UpdateState()
    {


        CheckSwitchState();
    }

    public override void FixedUpdateState()
    {

    }

    public override void ExitState()
    {
        Debug.Log("Leaving Dash");
    }
    public override void InitializeSubState()
    {
    }
    public override void CheckSwitchState()
    {
        SwitchState(factory.Idle());
    }
}
