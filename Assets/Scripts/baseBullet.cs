using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baseBullet : MonoBehaviour
{
    //Rigidbody rb;
    [SerializeField]
    protected float bulletSpeed = 40f;
    protected float bulletAccel;
    [SerializeField]
    protected float bulletSize = 0.5f;
    float bulletLife = 3f;
    float startTime;

    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody>();
        transform.localScale *= bulletSize;
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 3f, Color.red);
        if(Time.time - startTime > bulletLife)
        {
            Destroy(transform.gameObject);
        }
    }

    private void FixedUpdate()
    {
        transform.position += transform.forward * bulletSpeed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(transform.gameObject);
    }
}
