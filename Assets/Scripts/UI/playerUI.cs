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

        gravityCooldown = new Cooldown(gravitySlider);
        dashCooldown = new Cooldown(dashSlider);
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

