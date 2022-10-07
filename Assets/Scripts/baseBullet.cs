using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baseBullet : MonoBehaviour
{
    [SerializeField]
    protected float bulletSpeed = 10f;
    protected float bulletAccel;
    protected float bulletSize;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        transform.Translate(transform.forward * bulletSpeed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("here");
        Destroy(transform.gameObject);
    }
}
