using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UpdateReticleArgs : IGameEvent
{
    public bool CanPull { get; private set; }
    public bool CanPush { get; private set; }

    public UpdateReticleArgs(bool canPull, bool canPush)
    {
        CanPull = canPull;
        CanPush = canPush;
    }
}

[RequireComponent(typeof(Camera))]
public class WeaponRaycastHandler : GameEventUserObject
{
    [SerializeField] private float _range = 50f;
    [SerializeField] private AnimationCurve _fireRateCurve;
    [SerializeField, Range(1, 10)] private int _fireRateSteps = 1;

    private Camera _camera;
    private RaycastHit _hit;
    private bool _canPush;
    private bool _canPull;
    private float _lastKeyPosition;

    protected override void Awake()
    {
        _camera = GetComponent<Camera>();
        _lastKeyPosition = _fireRateCurve.keys[_fireRateCurve.length - 1].time;

        base.Awake();
    }

    public override void Subscribe()
    {
        EventManager.AddListener<WeaponFiredEventArgs>(FireWeapon);
    }

    public override void Unsubscribe()
    {
        EventManager.RemoveListener<WeaponFiredEventArgs>(FireWeapon);
    }

    private void Update()
    {
        UpdateMembers();
    }

    private void UpdateMembers() //Updates this frames hit raycast and other members
    {
        _canPull = false;
        _canPush = false;

        if(Physics.Raycast(_camera.transform.position, _camera.transform.forward, out _hit, _range))
        {
            if(_hit.transform.gameObject.layer == LayerMask.NameToLayer("MoveablePlane"))
            {
                MoveablePlane plane = _hit.transform.GetComponentInParent<MoveablePlane>();
                if(plane) 
                {
                    _canPush = plane.CanMove(FireType.Push);
                    _canPull = plane.CanMove(FireType.Pull);
                }
                else Debug.LogError("No MoveablePlane component found!");
            }
        }

        EventManager.TriggerEvent(new UpdateReticleArgs(_canPull, _canPush));
    }

    //this needs to be looked at again
    private IEnumerator UpdateFireRate(WeaponFiredEventArgs eventArgs, Transform hitTransform)
    {
        int mouseButton;

        switch(eventArgs.FireTypeArg)
        {
            case FireType.Push:
                mouseButton = 0;
                break;
            default:
                mouseButton = 1;
                break;
        }

        float increment = _lastKeyPosition / _fireRateSteps;
        float currentStep = increment;
        float waitTime = _fireRateCurve.Evaluate(increment);
        float timer = 0f;

        while(Input.GetMouseButton(mouseButton))
        {
            timer += Time.deltaTime;

            if(timer > waitTime)
            {
                if(!CheckRaycastForMatch(hitTransform)) break;

                HitResult hitResult = FireWeaponAgain(eventArgs);

                if(hitResult == HitResult.EndOfPath) break;

                currentStep = (currentStep + increment) > _lastKeyPosition ? _lastKeyPosition : (currentStep + increment);
                waitTime = _fireRateCurve.Evaluate(currentStep);
                timer = 0f;
            }

            yield return null;
        }
    }

    private bool CheckRaycastForMatch(Transform checkTransform)
    {
        if(_hit.transform.gameObject.layer == LayerMask.NameToLayer("MoveablePlane"))
        {
            return _hit.transform == checkTransform;
        }

        return false;
    }

    private void FireWeapon(WeaponFiredEventArgs eventArgs)
    {   
        if(_hit.transform)
        {
            if(_hit.transform.gameObject.layer == LayerMask.NameToLayer("MoveablePlane"))
            {
                MoveablePlane plane = _hit.transform.GetComponentInParent<MoveablePlane>();
                if(plane) 
                {
                    bool validTarget = (_canPull && eventArgs.FireTypeArg == FireType.Pull) || (_canPush && eventArgs.FireTypeArg == FireType.Push);
                    HandleHitSound(plane.HandleHit(new HitData(eventArgs.FireTypeArg, validTarget)), eventArgs.FireTypeArg);
                    StartCoroutine(UpdateFireRate(eventArgs, _hit.transform));
                }
                else Debug.LogError("No MoveablePlane component found!");
            }   
        }
    }

    private HitResult FireWeaponAgain(WeaponFiredEventArgs eventArgs)
    {
        if(_hit.transform)
        {
            if(_hit.transform.gameObject.layer == LayerMask.NameToLayer("MoveablePlane"))
            {
                MoveablePlane plane = _hit.transform.GetComponentInParent<MoveablePlane>();
                if(plane) 
                {
                    bool validTarget = (_canPull && eventArgs.FireTypeArg == FireType.Pull) || (_canPush && eventArgs.FireTypeArg == FireType.Push);
                    HitResult result = plane.HandleHit(new HitData(eventArgs.FireTypeArg, validTarget));
                    HandleHitSound(result, eventArgs.FireTypeArg);
                    return result;
                }
                else Debug.LogError("No MoveablePlane component found!");
            }
        }

        return HitResult.Failure;
    }

    private void HandleHitSound(HitResult hitResult, FireType fireType) //IK this is bad, but it may be good enough
    {
        switch(hitResult)
        {
            case HitResult.EndOfPath:
                AudioManager.Instance.PlayEndOfPathSound();
                break;
            case HitResult.Success:
                switch(fireType)
                {
                    case FireType.Pull:
                        AudioManager.Instance.PlayPullSound();
                        break;
                    case FireType.Push:
                        AudioManager.Instance.PlayPushSound();
                        break;
                }
                break;
            case HitResult.Failure:
                AudioManager.Instance.PlayDudSound();
                break;
        }
    }
}
