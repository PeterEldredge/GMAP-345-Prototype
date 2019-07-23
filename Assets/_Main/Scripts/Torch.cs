using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : MonoBehaviour
{
    [SerializeField] private Light[] _lights;
    [SerializeField] private ParticleSystem[] _particleSystems;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        if(_lights.Length == 0)
        {
            _lights = GetComponentsInChildren<Light>();
        }

        if(_particleSystems.Length == 0)
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
        }
    }

    public void On()
    {
        foreach(Light light in _lights)
        {
            light.enabled = true;
        }
        
        foreach(ParticleSystem pSystem in _particleSystems)
        {
            pSystem.Play();
        }
        
        if(_audioSource) _audioSource.Play();
    }

    public void Off()
    {
        foreach(Light light in _lights)
        {
            light.enabled = false;
        }
        
        foreach(ParticleSystem pSystem in _particleSystems)
        {
            pSystem.Stop();
        }
    }
}
