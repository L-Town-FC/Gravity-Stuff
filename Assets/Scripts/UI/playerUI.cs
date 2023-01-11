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
    TMP_Text ammoText;
    [SerializeField]
    GameObject currentGun;
    Cooldown gravityCooldown;

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

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void UpdateAmmoCount(int currentAmmo, int maxAmmo)
    {
        ammoText.text = currentAmmo + "/" + maxAmmo;
    }
}
