using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlphaChanger : GameEventUserObject
{
    [SerializeField] private Color _endingColor;

    private Image _panel;

    protected override void Awake()
    {
        _panel = GetComponent<Image>();
        _panel.enabled = false;

        base.Awake();
    }

    public override void Subscribe()
    {
        EventManager.AddListener<GameOverEventArgs>(StartFadeOut);
    }

    public override void Unsubscribe()
    {
        EventManager.RemoveListener<GameOverEventArgs>(StartFadeOut);
    }

    private void StartFadeOut(GameOverEventArgs args)
    {
        StartCoroutine(FadeOut(args.WaitTime));
    }

    private IEnumerator FadeOut(float waitTime)
    {
        float timer = 0;
        Color startingColor = _panel.color;
        _panel.enabled = true;

        while(timer < waitTime)
        {
            _panel.color = Color.Lerp(startingColor, _endingColor, timer / waitTime);
            timer += Time.deltaTime;

            yield return null;
        }

        _panel.color = _endingColor;
    }
}
