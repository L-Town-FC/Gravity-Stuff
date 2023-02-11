using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Netcode;
public class baseBullet : NetworkBehaviour
{
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

    float alpha1 = 1f;
    float alpha2 = 0.89f;

    Vector3 collisionPoint;

    VisualEffect shrapnelEffect;

    Collider[] collidersAtSpawn;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collidersAtSpawn = Physics.OverlapSphere(transform.position, 0.01f);
        IgnoreCollision();

        
    }

    // Start is called before the first frame update
    void Start()
    {
        //spawnTime = Time.time;
        rb.AddForce(transform.forward * bulletSpeed, ForceMode.VelocityChange);

        bulletCollider = GetComponent<Collider>();
        bulletRenderer = GetComponent<Renderer>();
        bulletLight = GetComponent<Light>();
        trail = GetComponent<TrailRenderer>();
        shrapnelEffect = GetComponent<VisualEffect>();
        transform.localScale *= bulletSize;
        startTime = Time.time;
        gradientColorKeys = trail.colorGradient.colorKeys;
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
            DestroyBulletServerRpc();
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "bubble shield") //lets bullets bounce off bubble shield. it looks cool so im keeping it
        {
            return;
        }

        rb.position = collisionPoint; //sets the object to the point of collision because it would normally bounce off the object and change position in the time this is called
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ; //ensures that once its in the right position, it no longer moves
        rb.velocity = Vector3.zero; //another check to make sure the bullet isnt moving
        shrapnelEffect.Play(); //plays the visual effect for shrapnel

        //used to make the bullet disappear on impact
        bulletLight.enabled = false;
        bulletCollider.enabled = false;
        bulletRenderer.enabled = false;
        trail.emitting = false;

        //starts the trail shrinking funciton
        shrinkTrail = true;

        //1 second was chosen arbitrarily. I didnt want the object sticking around too long because it was invisible and doing nothing but taking up memory, but wanted to give the shrink tail function time to run
        DestroyBulletServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyBulletServerRpc()
    {
        if (!IsOwner)
        {
            return;
        }
        GetComponent<NetworkObject>().Despawn(true);
        
        NetworkObject.Destroy(transform, 1f);
        //Destroy(transform.gameObject, 1f);
        
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
