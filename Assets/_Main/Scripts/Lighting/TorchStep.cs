using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TorchStep
{
    public float StartTime;
    public List<Light> Lights;
	public List<ParticleSystem> ParticleSystems;
}
