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
    private AudioSource _musicSource;
    private float _musicVolume;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        _musicSource = GetComponent<AudioSource>();
        _musicVolume = _musicSource.volume;
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

    public void StartMusic()
    {
        _musicSource.volume = _musicVolume;
        _musicSource.Play();
    }

    public void StartMusic(AudioClip clip)
    {
        StopCoroutine(_fadeRoutine);
        _musicSource.clip = clip;
        _musicSource.volume = _musicVolume;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _fadeRoutine = StartCoroutine(FadeOut(_musicSource, 3f));
    }

    private IEnumerator FadeOut(AudioSource source, float time)
    {
        float timer = 0f;
        float startingVolume = source.volume; 

        while(timer < time)
        {
            source.volume = Mathf.Lerp(startingVolume, 0f, timer / time);
            timer += Time.deltaTime;
            yield return null;
        }

        source.Stop();
    }


    private IEnumerator AddBackToQueue(AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length + .1f);

        _audioSources.Enqueue(audioSource);
    }
}
