public abstract class PlayerBaseState
{
    protected PlayerStateMachine ctx;
    protected PlayerStateFactory factory;

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        ctx = currentContext;
        factory = playerStateFactory;
    }
    

    public abstract void EnterState();
    public abstract void UpdateState();

    public abstract void FixedUpdateState();

    public abstract void ExitState();
    public abstract void CheckSwitchState();
    public abstract void InitializeSubState();

    void UpdateStates() { }

    protected void SwitchState(PlayerBaseState newState) {
        //current state exits state
        ExitState();

        //new state enters state
        newState.EnterState();

        ctx._currentState = newState;
    }

    protected void SetSuperState() { }

    protected void SetSubState() { }
}
