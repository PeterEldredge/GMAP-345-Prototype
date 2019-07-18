using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBox : MonoBehaviour 
{
    [SerializeField] private float _spherecastRadius = .5f;
    public float SpherecastRadius { get { return _spherecastRadius; } }

    private void Awake()
    {
        _spherecastRadius *= .98f;
        transform.localScale *= .98f;
    }
}
