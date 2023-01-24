using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public GameObject player; //set this when spawning UI

    PlayerStateMachine playerStateMachine;

    TMP_Text ammoText;
    Slider gravitySlider;
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
        ammoText = transform.Find("AmmoCount").Find("ClipSize").GetComponent<TMP_Text>();
        gravitySlider = transform.Find("Cooldowns").Find("GravityCooldown").GetComponent<Slider>();
        dashSlider = transform.Find("Cooldowns").Find("DashCooldown").GetComponent<Slider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //this needs to be called in start instead of awake because of how the player variable is set AFTER its instantiated
        playerStateMachine = player.GetComponent<PlayerStateMachine>();

        gravityCooldown = new Cooldown(gravitySlider);
        dashCooldown = new Cooldown(dashSlider);
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
}

public class Cooldown
{
    public Slider slider; //reference to desired slider

    public Cooldown(Slider _slider)
    {
        slider = _slider;
    }

    public void UpdateSlider(float _cooldownTime, float _lastUse)
    {
        slider.value = Mathf.InverseLerp(_lastUse, _lastUse + _cooldownTime, Time.time);
    }
}

