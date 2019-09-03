using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct GameOverEventArgs : IGameEvent
{
    public float WaitTime { get; private set; }

    public GameOverEventArgs(float waitTime)
    {
        WaitTime = waitTime;
    }
}

public class EndGameTrigger : MonoBehaviour
{
    [SerializeField] private float _time;

    private void OnTriggerEnter(Collider other)
    {
        AudioManager.Instance.StopMusic(_time);
        EventManager.TriggerEvent(new GameOverEventArgs(_time));
        PauseMenu.Instance.GameIsPaused = true; // Complete Hack

        StartCoroutine(Wait(_time + 2f));
    }

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);

        PauseMenu.Instance.GameIsPaused = false; // Complete Hack
        SceneManager.LoadScene("MainMenu");
    }
}
