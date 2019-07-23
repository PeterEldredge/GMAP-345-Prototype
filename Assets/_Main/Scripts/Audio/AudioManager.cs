using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private int _numOfSources = 6;
    [SerializeField] private AudioClip _pullSound = null;
    [SerializeField] private AudioClip _pushSound = null;
    [SerializeField] private AudioClip _dudSound = null;
    [SerializeField] private AudioClip _endOfPathSound = null;

    public static AudioManager Instance { get; private set; }

    private Queue<AudioSource> _audioSources;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        Instance._audioSources = new Queue<AudioSource>();

        for(int i = 0; i < _numOfSources; i++)
        {
            _audioSources.Enqueue((AudioSource)gameObject.AddComponent(typeof(AudioSource)));
        }
    }

    internal void PlayTorchOnSound()
    {
        throw new NotImplementedException();
    }

    public void PlayPushSound() //Make Generic play functions later
    {
        AudioSource audioSource = _audioSources.Dequeue();
        audioSource.clip = _pushSound;
        audioSource.Play();
        StartCoroutine(AddBackToQueue(audioSource));
    }

    public void PlayPullSound()
    {
        AudioSource audioSource = _audioSources.Dequeue();
        audioSource.clip = _pullSound;
        audioSource.Play();
        StartCoroutine(AddBackToQueue(audioSource));
    }

    public void PlayDudSound()
    {
        AudioSource audioSource = _audioSources.Dequeue();
        audioSource.clip = _dudSound;
        audioSource.Play();
        StartCoroutine(AddBackToQueue(audioSource));
    }

    public void PlayEndOfPathSound()
    {
        AudioSource audioSource = _audioSources.Dequeue();
        audioSource.clip = _endOfPathSound;
        audioSource.Play();
        StartCoroutine(AddBackToQueue(audioSource));
    }

    private IEnumerator AddBackToQueue(AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length + .1f);

        _audioSources.Enqueue(audioSource);
    }
}
