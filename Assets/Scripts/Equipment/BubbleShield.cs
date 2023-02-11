using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BubbleShield : MonoBehaviour, IEquipment
{
    Rigidbody rb;
    Vector3 gravityDir;
    Vector3 trajectory;
    float gravityForce;
    float maxSize = 10f;
    float sizeChangeRate = 0.1f;
    [SerializeField]
    float duration = 10f;
    bool isFalling = true;
    Vector3 currentScale = Vector3.zero;
    float initialSize = 0.25f;

    [SerializeField]
    Material bubbleShieldMaterial;
    [SerializeField]
    Material initialMaterial;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 0f;
        Destroy(this.gameObject, duration);
        transform.GetComponent<Renderer>().material = initialMaterial;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.isKinematic = false;

        rb.AddForce(trajectory.normalized * 30f, ForceMode.Impulse);
        transform.localScale = Vector3.one * initialSize;
        
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
            }

            transform.localScale = currentScale;
        }
        else
        {
            transform.localScale = Vector3.one * initialSize;
        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(gravityDir.normalized * gravityForce);
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
        isFalling = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezePosition;
        transform.GetComponent<Renderer>().material = bubbleShieldMaterial;
    }
}
