public class PlayerStateFactory
{
    PlayerStateMachine context;
    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        context = currentContext;

    }
    public PlayerBaseState Idle() {
        return new PlayerIdleState(context, this);
    }
    public PlayerBaseState SwitchGravity() {
        return new PlayerGravitySwitchState(context, this);
    }
}
