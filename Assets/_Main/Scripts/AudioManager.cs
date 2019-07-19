using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip _pullSound = null;
    [SerializeField] private AudioClip _pushSound = null;
    [SerializeField] private AudioClip _dudSound = null;
    [SerializeField] private AudioClip _endOfPathSound = null;

    public static AudioManager Instance { get; private set; }

    private AudioSource _audioSource;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        Instance._audioSource = GetComponent<AudioSource>();
    }

    public void PlayPushSound()
    {
        Instance._audioSource.clip = _pushSound;
        Instance._audioSource.Play();
    }

    public void PlayPullSound()
    {
        Instance._audioSource.clip = _pullSound;
        Instance._audioSource.Play();
    }

    public void PlayDudSound()
    {
        Instance._audioSource.clip = _dudSound;
        Instance._audioSource.Play();
    }

    public void PlayEndOfPathSound()
    {
        Instance._audioSource.clip = _endOfPathSound;
        Instance._audioSource.Play();
    }
}
