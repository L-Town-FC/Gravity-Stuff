using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleShield : MonoBehaviour
{
    Rigidbody rb;
    public Vector3 gravityDir;
    float gravity = -.75f; //matches players. Need to change these together. Should probably have a global one
    float maxSize = 10f;
    float sizeChangeRate = 0.1f;
    [SerializeField]
    float duration = 10f;
    bool isFalling = true;
    Vector3 velocity;
    Vector3 currentScale = Vector3.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 0f;
        Destroy(this.gameObject, duration);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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
    }

    private void FixedUpdate()
    {
        Gravity();
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    void Gravity()
    {
        if (isFalling)
        {
            velocity += gravityDir * gravity;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isFalling = false;
        velocity = Vector3.zero;
        rb.isKinematic = true;
    }
}
