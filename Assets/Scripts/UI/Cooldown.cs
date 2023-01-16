using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cooldown
{
    //can probably get rid of cooldownTime and lastUse but dont care rn
    public float cooldownTime = 0f; //time between things
    public float lastUse = 0f; //last time thing was done
    public Slider slider; //reference to desired slider

    public Cooldown(float _cooldownTime, float _lastUse, Slider _slider)
    {
        cooldownTime = _cooldownTime;
        lastUse = _lastUse;
        slider = _slider;
    }

    public void UpdateSlider(float _cooldownTime, float _lastUse)
    {
        cooldownTime = _cooldownTime;
        lastUse = _lastUse;
        slider.value = Mathf.InverseLerp(lastUse, lastUse + cooldownTime, Time.time);
    }
}
