using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Netcode;

public class baseBullet : NetworkBehaviour
{
    //POOLED DYNAMIC SPAWNING
    //predictive spawning isnt currently in unity netcode. Need to look into object pooling instead
    //my current idea which probably makes no sense
    //spawn 100 bullets far away with everything disabled in a box.
    //have the box constantly check if there are 100 bullets in it, if not, spawn more inside it
    //when a player shoots move a bullet to his gun, enable it, and add a force

    //https://docs.unity3d.com/Packages/com.unity.netcode@1.0/manual/prediction.html
    //https://docs-multiplayer.unity3d.com/netcode/current/basics/object-spawning

    bool initialized;

    Rigidbody rb;
    Collider bulletCollider;
    Renderer bulletRenderer;
    Light bulletLight;
    TrailRenderer trail;
    [SerializeField]
    protected float bulletSpeed = 5f;
    protected float bulletAccel;
    [SerializeField]
    protected float bulletSize = 0.5f;
    float bulletLife = 5f;
    float bulletShrinkTime = 4f;
    float startTime;
    bool shrinkTrail = false;
    GradientColorKey[] gradientColorKeys;
    GradientColorKey[] initialColorKeys;
    float alpha1 = 1f;
    float alpha2 = 0.89f;

    Vector3 collisionPoint;

    VisualEffect shrapnelEffect;
    
    Collider[] collidersAtSpawn;

    private void Awake()
    {
        if (!initialized)
        {
            rb = GetComponent<Rigidbody>();
            bulletCollider = GetComponent<Collider>();
            bulletRenderer = GetComponent<Renderer>();
            bulletLight = GetComponent<Light>();
            trail = GetComponent<TrailRenderer>();
            shrapnelEffect = GetComponent<VisualEffect>();
            transform.localScale *= bulletSize;
            startTime = Time.time;
            initialColorKeys = gradientColorKeys = trail.colorGradient.colorKeys;
            startTime = Time.time;
            bulletLight.enabled = true;
            bulletCollider.enabled = true;
            bulletRenderer.enabled = true;
            trail.emitting = true;
            shrinkTrail = false;


        }
        collidersAtSpawn = Physics.OverlapSphere(transform.position, 0.01f);
        IgnoreCollision();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        startTime = Time.time;
        bulletLight.enabled = true;
        bulletCollider.enabled = true;
        bulletRenderer.enabled = true;
        trail.emitting = true;
        shrinkTrail = false;
        bulletCollider.enabled = true;
        bulletRenderer.enabled = true;

    }
    
    // Update is called once per frame
    void Update()
    {

        //Debug.DrawRay(transform.position, transform.forward * 0.1f, Color.red);
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit);
        collisionPoint = hit.point;
        if(Time.time - startTime > bulletLife)
        {
            DestroyBulletServer();
        }

        if(Time.time - startTime > bulletShrinkTime)
        {
            shrinkTrail = true;
        }

        if(shrinkTrail)
        {
            ShrinkTrail();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void InitialForceServerRpc()
    {
        rb.AddForce(transform.forward * bulletSpeed, ForceMode.VelocityChange);
        
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.transform.tag == "bubble shield") //lets bullets bounce off bubble shield. it looks cool so im keeping it
        {
            return;
        }

        StartCoroutine("StartDestroy");

        rb.position = collisionPoint; //sets the object to the point of collision because it would normally bounce off the object and change position in the time this is called
        rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation; //ensures that once its in the right position, it no longer moves
        rb.velocity = Vector3.zero; //another check to make sure the bullet isnt moving
        shrapnelEffect.Play(); //plays the visual effect for shrapnel

        //used to make the bullet disappear on impact
        bulletLight.enabled = false;
        bulletCollider.enabled = false;
        bulletRenderer.enabled = false;
        trail.emitting = false;

        //starts the trail shrinking funciton
        shrinkTrail = true;
    }

    //Need to use a coroutine in order to wait 1 sec after collison because network despawn doesnt have a built in wait function
    //1 sec was chosen arbitrarily in order to give visual effect time to occur
    IEnumerator StartDestroy()
    {
        yield return new WaitForSeconds(1f);
        DestroyBulletServer();
    }

    void DestroyBulletServer()
    {
        transform.GetComponent<NetworkObject>().Despawn();
        //NetworkObjectPool.Singleton.ReturnNetworkObject(transform.GetComponent<NetworkObject>(), )
    }

    //the tail size is normally constant during its lifetime
    //this slowly changes the gradients for transparency so end of the tail starts becoming more transparent until the whole tail is transparent
    //tail disappears slowly over time now instead of all at once which could be jarring
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

    private void OnDisable()
    {
        startTime = Mathf.Infinity;
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        trail.Clear();
        alpha1 = 1f;
        alpha2 = 0.89f;
        gradientColorKeys = initialColorKeys;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { gradientColorKeys[0], gradientColorKeys[1] },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha1, 0f), new GradientAlphaKey(alpha2, 0.75f), new GradientAlphaKey(0f, 0.9f) }
        );

        trail.colorGradient = gradient;
        shrinkTrail = false;
    }

    //sets the color gradient for the bullets trail
    void SetGradient()
    {

    }

    //checks if the bullet spawns inside a bubble shield, i.e. the player is inside the bubble shield
    //Before this, unity physics would take over and launch the bullet out of the collider in a random direction. by ignoring the collider, the player can should out of the bubble shield just fine
    void IgnoreCollision()
    {
        foreach(Collider col in collidersAtSpawn)
        {
            if(col.tag == "bubble shield")
            {
                Physics.IgnoreCollision(transform.GetComponent<Collider>(), col);
            }
        }
    }



}
