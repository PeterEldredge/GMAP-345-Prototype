using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveablePlane : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    public List<Axis> Axes;
}
