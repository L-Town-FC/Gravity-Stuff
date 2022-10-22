using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class playerUI : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    [SerializeField]
    RectTransform gravityCooldown;
    Rect gravityCooldownRect;
    float gravityChangeCooldownTime;
    float gravityChangeTime = 0f;
    float barMaxWidth;
    [Range(0, 1)]
    public static float barWidth;

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
        gravityCooldownRect = gravityCooldown.rect;
        barMaxWidth = gravityCooldownRect.width;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        barWidth = Mathf.InverseLerp(gravityChangeTime, gravityChangeTime + gravityChangeCooldownTime, Time.time);
        gravityCooldown.sizeDelta = new Vector2(barWidth * barMaxWidth, gravityCooldownRect.height);
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
