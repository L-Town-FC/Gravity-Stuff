using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class playerUI : MonoBehaviour
{
    [SerializeField]
    GameObject player;
    PlayerStateMachine playerStateMachine;

    [SerializeField]
    TMP_Text ammoText;
    [SerializeField]
    GameObject currentGun;
    [SerializeField]
    Slider gravitySlider;
    [SerializeField]
    Slider dashSlider;

    Cooldown gravityCooldown;
    Cooldown dashCooldown;

    private void OnEnable()
    {
        baseGun.ammoUpdate += UpdateAmmoCount;
    }

    private void OnDisable()
    {
        baseGun.ammoUpdate -= UpdateAmmoCount;
    }

    private void Awake()
    {
        playerStateMachine = player.GetComponent<PlayerStateMachine>();

        gravityCooldown = new Cooldown(playerStateMachine._gravityChangeCooldownTime, playerStateMachine._lastGravityChangeTime, gravitySlider);
        dashCooldown = new Cooldown(playerStateMachine._dashCooldownTime, playerStateMachine._lastDashTime, dashSlider);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gravityCooldown.UpdateSlider(playerStateMachine._gravityChangeCooldownTime ,playerStateMachine._lastGravityChangeTime);
        dashCooldown.UpdateSlider(playerStateMachine._dashCooldownTime, playerStateMachine._lastDashTime);
    }

    void UpdateAmmoCount(int currentAmmo, int maxAmmo)
    {
        ammoText.text = currentAmmo + "/" + maxAmmo;
    }

    void UpdateCooldowns()
    {

    }
}

