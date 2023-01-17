using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : MonoBehaviour
{
    //Created a dictionary so all gravitationally bound objects can be easily added, updated, and removed. Needed a way to track all objects that wasn't too intensive
    //This seemed like it required the lowest amount of information to be tracked and the least amount of refernce calls
    Dictionary<int, Rigidbody> keyValuePairs = new Dictionary<int, Rigidbody>();
    [SerializeField]
    float baseGravitationalForce = 20f;
    Vector3 center;

    // Start is called before the first frame update
    void Start()
    {
        center = transform.position;
    }

    // adds a force to objects within the black holes volume that pulls it towards the center
    void Update()
    {
        foreach(KeyValuePair<int, Rigidbody> gravityBoundObject in keyValuePairs)
        {
            //use magnitude squared as it is a less intensive equation than magnitude
            //also the distance would have to be squared anyways for the gravity equation

            if(gravityBoundObject.Value == null)
            {
                keyValuePairs.Remove(gravityBoundObject.Key);
                return;
            }

            Vector3 objectPos = gravityBoundObject.Value.position; //this value is used twice so I save the value instead of asking for the value twice from the rigid body
            float distanceSquared = (center - objectPos).sqrMagnitude;
            gravityBoundObject.Value.AddForce((center - objectPos) * (baseGravitationalForce/distanceSquared));
        }
    }

    //adds any objects entering the black holes volume into a list that will be updated later
    private void OnTriggerEnter(Collider other)
    {
        keyValuePairs.Add(other.attachedRigidbody.GetInstanceID(), other.attachedRigidbody);   
    }

    //removed objects that leave the black holes volume as they are no longer bound by it
    private void OnTriggerExit(Collider other)
    {
        keyValuePairs.Remove(other.attachedRigidbody.GetInstanceID());
    }
}
