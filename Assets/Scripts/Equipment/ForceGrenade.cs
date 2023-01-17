using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceGrenade : MonoBehaviour
{
    Vector3 gravityDir;
    float gravityForce = 50f;
    float forceRadius = 15f;
    float force = 100f;
    Rigidbody rb;
    LayerMask layerMask = new LayerMask();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        layerMask.value = LayerMask.GetMask("Ground");
    }

    // Start is called before the first frame update
    void Start()
    {
        if(gravityDir == Vector3.zero)
        {
            gravityDir = Vector3.down;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rb.AddForce(gravityDir * gravityForce);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Want to eliminate cases where a force shouldnt be applied to objects in the grenades area
        //easiest check is if a rigidbody exists because a force cant be applied without it
        //then checking if any walls of "ground" obstruct the grenade from the object

        rb.constraints = RigidbodyConstraints.FreezeAll;

        Collider[] colliders;
        colliders = Physics.OverlapSphere(transform.position, forceRadius);

        foreach (Collider collider in colliders)
        {
            if (collider.attachedRigidbody == null)
            {
                continue;
            }

            if(Physics.Linecast(transform.position, collider.transform.position, layerMask))
            {
                continue;
            }

            Vector3 dir = collider.transform.position - transform.position;
            collider.attachedRigidbody.AddForce(force * dir, ForceMode.Impulse);
        }
    }
}
