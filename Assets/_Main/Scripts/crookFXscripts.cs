using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crookFXscripts : MonoBehaviour

{
    public ParticleSystem blueFlash;
    public ParticleSystem redFlash;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            blueFlash.Play();


        if (Input.GetMouseButtonDown(1))
            redFlash.Play();

    }
}
