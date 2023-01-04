using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : MonoBehaviour
{
    Dictionary<int, Rigidbody> keyValuePairs = new Dictionary<int, Rigidbody>();
    float baseGravitationalForce = 200f;
    Vector3 center;

    // Start is called before the first frame update
    void Start()
    {
        center = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        foreach(KeyValuePair<int, Rigidbody> test in keyValuePairs)
        {
            float distanceSquared = (center - test.Value.position).sqrMagnitude;
            test.Value.AddForce((center - test.Value.position) * (baseGravitationalForce/distanceSquared));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        keyValuePairs.Add(other.attachedRigidbody.GetInstanceID(), other.attachedRigidbody);
    }

    private void OnTriggerExit(Collider other)
    {
        keyValuePairs.Remove(other.attachedRigidbody.GetInstanceID());
    }
}
