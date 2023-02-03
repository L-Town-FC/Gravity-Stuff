using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEquipment
{
    void Gravity(float gravity, Vector3 gravityDir) { }

    void Trajectory(Vector3 trajectory) { }
}
