using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerStateManager : MonoBehaviour
{
    public enum PlayerState {_default, dashing, flipping, reloading, shooting};
    public static PlayerState currentPlayerState = PlayerState._default;

    public static Queue<PlayerState> playerStatesQueue = new Queue<PlayerState>();
    
    //need to think about how to do this
    //maybe two variables, new state and current state
    //when another scripts tries to initiate a state change this script will check if its an allowed state change
    //if it is then the current state will change
    //if not, the state will remain the same

    //should look into how to make a state tree

    private void Awake()
    {
        currentPlayerState = PlayerState._default;
    }

    private void OnEnable()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //print(currentPlayerState);
    }

    private void LateUpdate()
    {
        if(playerStatesQueue.Count > 0)
        {
            currentPlayerState = playerStatesQueue.Dequeue();
        }

        playerStatesQueue.Clear();
    }

}
