using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReticleUpdater : GameEventUserObject
{
    [SerializeField] private Sprite _defaultReticle;
    [SerializeField] private Sprite _pushableReticle;
    [SerializeField] private Sprite _pullableReticle;
    [SerializeField] private Sprite _bothableReticle;

    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
    }
  
    public override void Subscribe() 
    {
        EventManager.AddListener<UpdateReticleArgs>(UpdateReticle);
    }

    public override void Unsubscribe()
    {
        EventManager.RemoveListener<UpdateReticleArgs>(UpdateReticle);
    }

    private void UpdateReticle(UpdateReticleArgs reticleArgs)
    {
        if(reticleArgs.CanPull && reticleArgs.CanPush)
        {
            _image.sprite = _bothableReticle;
        }
        else if (reticleArgs.CanPull)
        {
            _image.sprite = _pullableReticle;
        }
        else if (reticleArgs.CanPush)
        {
            _image.sprite = _pushableReticle;
        }
        else
        {
            _image.sprite = _defaultReticle;
        }
    }
}
