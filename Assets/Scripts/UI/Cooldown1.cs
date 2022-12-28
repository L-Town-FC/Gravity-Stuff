using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cooldown1: MonoBehaviour
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
        changeCooldownTime = playerStateMachine._dashCooldownTime;
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

    void UpdateDashTime(float currentTime)
    {
        changeTime = currentTime;
    }

    private void OnEnable()
    {
        PlayerStateMachine.dashed += UpdateDashTime;
    }

    private void OnDisable()
    {
        PlayerStateMachine.dashed -= UpdateDashTime;
    }
}
