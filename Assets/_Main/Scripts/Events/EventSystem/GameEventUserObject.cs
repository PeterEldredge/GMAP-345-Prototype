using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameEventUserObject : MonoBehaviour, IUseGameEvents
{
    protected virtual void Awake()
    {
        Subscribe();
    }

    protected virtual void OnDestroy()
    {
        Unsubscribe();
    }

    public abstract void Subscribe();
    public abstract void Unsubscribe();
}
