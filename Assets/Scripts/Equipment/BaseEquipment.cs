using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEquipment : MonoBehaviour
{
    //these will be the same for every equipment that is created so this class is required for all created equipments

    public Vector3 gravityDir = new Vector3();
    public Vector3 trajectory = new Vector3();
    public float gravityForce = 50f;
}
