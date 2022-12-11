using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class playerUI : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    [SerializeField]
    Slider gravityCoolDownSlider;
    [SerializeField]
    Image gravityCooldownFillColor;
    bool gravityCooldownFinished;
    float gravityChangeCooldownTime;
    float gravityChangeTime = 0f;

    [SerializeField]
    TMP_Text ammoText;
    [SerializeField]
    GameObject currentGun;

    private void OnEnable()
    {
        playerMovement.gravityChanged += UpdateGravityChangeTime;
        baseGun.ammoUpdate += UpdateAmmoCount;
    }

    private void OnDisable()
    {
        playerMovement.gravityChanged -= UpdateGravityChangeTime;
        baseGun.ammoUpdate -= UpdateAmmoCount;
    }

    private void Awake()
    {
        gravityChangeCooldownTime = playerMovement.gravityChangeCooldownTime;
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
        gravityCoolDownSlider.value = Mathf.InverseLerp(gravityChangeTime, gravityChangeTime + gravityChangeCooldownTime, Time.time);
    }

    void UpdateGravityChangeTime(float currentTime)
    {
        gravityChangeTime = currentTime;
    }

    void UpdateAmmoCount(int currentAmmo, int maxAmmo)
    {
        ammoText.text = currentAmmo + "/" + maxAmmo;
    }
}
