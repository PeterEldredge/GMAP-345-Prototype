using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchSystem : MonoBehaviour
{
    [SerializeField] private List<TorchStep> _torchSteps;

    [SerializeField] private Color _startingAmbientColor;
    [SerializeField] private Color _endingAmbientColor;
    
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        RenderSettings.ambientLight = _startingAmbientColor;

        foreach(TorchStep torchStep in _torchSteps)
        {
            foreach(Torch torch in torchStep.Torches)
            {
                torch.Off();
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

            foreach(Torch torch in torchStep.Torches)
            {
                torch.On();
            }

            RenderSettings.ambientLight = Color.Lerp(_startingAmbientColor, _endingAmbientColor, (i + 1f) / _torchSteps.Count);
        }

        AudioManager.Instance.StartMusic();
    }
}