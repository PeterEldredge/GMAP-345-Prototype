using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchSystem : MonoBehaviour
{
    [SerializeField] private List<TorchStep> _torchSteps;

    [SerializeField] private Color _startingAmbientColor;
    [SerializeField] private Color _endingAmbientColor;

    private void Awake()
    {
        RenderSettings.ambientLight = _startingAmbientColor;

        foreach(TorchStep torchStep in _torchSteps)
        {
            foreach(Light light in torchStep.Lights)
            {
                light.enabled = false;
            }

            foreach(ParticleSystem pSystem in torchStep.ParticleSystems)
            {
                pSystem.Stop();
            }
        }

        StartCoroutine(LightTorches());
    }

    private IEnumerator LightTorches()
    {
        for(int i = 0; i < _torchSteps.Count; i++)
        {
            TorchStep torchStep = _torchSteps[i];

            yield return new WaitForSeconds(torchStep.StartTime);

            foreach(Light light in torchStep.Lights)
            {
                light.enabled = true;
            }

            foreach(ParticleSystem pSystem in torchStep.ParticleSystems)
            {
                pSystem.Play();
            }

            RenderSettings.ambientLight = Color.Lerp(_startingAmbientColor, _endingAmbientColor, (i + 1f) / _torchSteps.Count);
        }
    }
}