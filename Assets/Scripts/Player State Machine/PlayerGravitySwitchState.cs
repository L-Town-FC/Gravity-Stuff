using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravitySwitchState : PlayerBaseState
{
    public PlayerGravitySwitchState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    :base (currentContext, playerStateFactory){ }

    public override void EnterState()
    {
        Debug.Log("Flipping Gravity");
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
        
    }
    public override void InitializeSubState()
    {

    }
    public override void CheckSwitchState()
    {
        //random thing
        if(Mathf.RoundToInt(Time.time) == 5)
        {
            SwitchState(factory.Idle());
        }
    }
}
