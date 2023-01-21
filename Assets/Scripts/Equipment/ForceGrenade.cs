using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseEquipment))]
public class ForceGrenade : MonoBehaviour
{
    Vector3 gravityDir;
    Vector3 trajectory;
    float gravityForce;
    float forceRadius = 15f;
    float force = 100f;
    Rigidbody rb;
    LayerMask layerMask = new LayerMask();

    BaseEquipment baseEquipment; //class that carries variables that all equipment will use to keep stuff consistent and easy to manage

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        layerMask.value = LayerMask.GetMask("Ground");
        baseEquipment = GetComponent<BaseEquipment>();
    }

    // Start is called before the first frame update
    void Start()
    {
        gravityDir = baseEquipment.gravityDir;
        trajectory = baseEquipment.trajectory;
        gravityForce = baseEquipment.gravityForce;

        if(gravityDir == Vector3.zero)
        {
            gravityDir = Vector3.down;
        }

        rb.AddForce(trajectory.normalized * 30f, ForceMode.Impulse);
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

        Destroy(transform.gameObject); //change this to wait for visual effect to complete
    }
}
