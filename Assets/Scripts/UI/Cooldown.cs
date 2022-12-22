using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cooldown: MonoBehaviour
{
    [SerializeField]
    GameObject player;

    PlayerStateMachine playerStateMachine;

    [SerializeField]
    Slider cooldownSlider;

    float changeCooldownTime;

    float changeTime = 0f;

    private void Awake()
    {
        playerStateMachine = player.GetComponent<PlayerStateMachine>();
        changeCooldownTime = playerStateMachine._gravityChangeCooldownTime;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCooldowns();
    }

    void UpdateCooldowns()
    {
        cooldownSlider.value = Mathf.InverseLerp(changeTime, changeTime + changeCooldownTime, Time.time);
    }

    void UpdateGravityChangeTime(float currentTime)
    {
        changeTime = currentTime;
    }

    private void OnEnable()
    {
        PlayerStateMachine.gravityChanged += UpdateGravityChangeTime;
    }

    private void OnDisable()
    {
        playerMovement.gravityChanged -= UpdateGravityChangeTime;
    }
}
