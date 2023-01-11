using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleShield : MonoBehaviour
{
    Rigidbody rb;
    public Vector3 gravityDir;
    public Vector3 trajectory;
    float gravity = -.75f; //matches players. Need to change these together. Should probably have a global one
    float maxSize = 10f;
    float sizeChangeRate = 0.1f;
    [SerializeField]
    float duration = 10f;
    bool isFalling = true;
    Vector3 currentScale = Vector3.zero;
    public Collider playerCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 0f;
        Destroy(this.gameObject, duration);
        rb.AddForce(transform.forward * 20f, ForceMode.Impulse);
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.AddForce(trajectory.normalized * 30f, ForceMode.Impulse);
        transform.localScale = Vector3.one * 0.05f;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, trajectory.normalized * 5f, Color.black);
        if (!isFalling)
        {
            if(currentScale.x < 10f)
            {
                currentScale += Vector3.one * sizeChangeRate;
            }
            else
            {
                currentScale = Vector3.one * maxSize;
                transform.GetComponent<Collider>().isTrigger = true;
            }

            transform.localScale = currentScale;
        }
        else
        {
            transform.localScale = Vector3.one * 0.05f;
        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(-gravityDir.normalized * 50f);
    }


    private void OnCollisionEnter(Collision collision)
    {
        print(collision.collider.name);
        isFalling = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezePosition;
    }

    public void IgnorePlayer(CapsuleCollider temp)
    {
        Physics.IgnoreCollision(transform.GetComponent<Collider>(), temp);
    }
}
