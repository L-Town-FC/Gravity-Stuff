using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerUI : MonoBehaviour
{
    [SerializeField]
    RectTransform gravityCooldown;
    Rect gravityCooldownRect;
    float gravityChangeCooldownTime;
    float gravityChangeTime = 0f;
    float barMaxWidth;
    [Range(0, 1)]
    public static float barWidth;

    private void OnEnable()
    {
        player.gravityChanged += UpdateGravityChangeTime;
    }

    private void OnDisable()
    {
        player.gravityChanged -= UpdateGravityChangeTime;
    }

    private void Awake()
    {
        gravityChangeCooldownTime = player.gravityChangeCooldownTime;
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
}
