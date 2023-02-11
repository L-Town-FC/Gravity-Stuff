using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ForceGrenade : MonoBehaviour, IEquipment
{
    Vector3 gravityDir;
    Vector3 trajectory;
    float gravityForce;
    float forceRadius = 5f;
    [SerializeField]
    float force = 75f; //may want to also set players air drag lower for a small window after affected by this
    Rigidbody rb;
    LayerMask layerMask = new LayerMask();
    VisualEffect pulse;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        layerMask.value = LayerMask.GetMask("Ground");
        pulse = GetComponent<VisualEffect>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.isKinematic = false;

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

    public void Gravity(float _gravity, Vector3 _gravityDir)
    {
        gravityForce = _gravity;
        gravityDir = _gravityDir;
    }

    public void Trajectory(Vector3 _trajectory)
    {
        trajectory = _trajectory;
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

        //disables collider and mesh so it cant be seen after exlposion
        transform.GetComponent<MeshRenderer>().enabled = false;
        transform.GetComponent<Collider>().enabled = false;

        pulse.Play(); //play the vfx

        Destroy(transform.gameObject, 0.21f); //0.21s is slightly longer than the vfx plays for
    }
}
