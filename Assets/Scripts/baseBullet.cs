using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baseBullet : MonoBehaviour
{
    Collider bulletCollider;
    Renderer bulletRenderer;
    TrailRenderer trail;
    [SerializeField]
    protected float bulletSpeed = 40f;
    protected float bulletAccel;
    [SerializeField]
    protected float bulletSize = 0.5f;
    float bulletLife = 3f;
    float startTime;
    bool shrinkTrail = false;
    GradientColorKey[] gradientColorKeys;

    float alpha1 = 1f;
    float alpha2 = 0.89f;

    // Start is called before the first frame update
    void Start()
    {
        bulletCollider = GetComponent<Collider>();
        bulletRenderer = GetComponent<Renderer>();
        trail = GetComponent<TrailRenderer>();
        transform.localScale *= bulletSize;
        startTime = Time.time;
        gradientColorKeys = trail.colorGradient.colorKeys;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 3f, Color.red);
        if(Time.time - startTime > bulletLife)
        {
            Destroy(transform.gameObject);
        }

        if(shrinkTrail)
        {
            ShrinkTrail();
        }



    }

    private void FixedUpdate()
    {
        transform.position += transform.forward * bulletSpeed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        bulletSpeed = 0f;
        bulletCollider.enabled = false;
        bulletRenderer.enabled = false;
        trail.emitting = false;
        shrinkTrail = true;
        
        float timeDelay = 5f;
        Destroy(transform.gameObject, timeDelay);
    }

    void ShrinkTrail()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { gradientColorKeys[0], gradientColorKeys[1] },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha1, 0f), new GradientAlphaKey(alpha2, 0.75f), new GradientAlphaKey(0f, 0.9f) }
        );

        alpha1 -= 0.007f;
        alpha2 -= 0.007f;

        trail.colorGradient = gradient;
    }

}
