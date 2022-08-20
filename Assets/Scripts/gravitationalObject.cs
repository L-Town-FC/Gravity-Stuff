using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gravitationalObject : MonoBehaviour
{
    //very weird stuff happening with this
    //player sometimes treats trigger as a solid collider
    HashSet<Transform> transforms = new HashSet<Transform>();
    Vector3 center;

    // Start is called before the first frame update
    void Start()
    {
        center = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        transforms.Add(other.transform);
    }

    private void OnTriggerStay(Collider other)
    {
        foreach(Transform _transform in transforms)
        {
            if(_transform.gameObject.TryGetComponent(out player _player))
            {
                _player.up = _transform.position - center;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (transforms.Contains(other.transform))
        {
            transforms.Remove(other.transform);
        }
    }

}
