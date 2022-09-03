using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotationTest : MonoBehaviour
{
    public bool yRotation = false;
    public bool xRotation = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (yRotation)
        {
        transform.Rotate(new Vector3(0f, 1f, 0f), Space.Self);

        }

        if (xRotation)
        {
        transform.Rotate(new Vector3(1f, 0f, 0f), Space.World);

        }
    }
}
