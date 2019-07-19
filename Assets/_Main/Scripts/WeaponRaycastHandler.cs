using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class WeaponRaycastHandler : GameEventUserObject
{
    [SerializeField] private float _range = 50f;
    [SerializeField] private AnimationCurve _fireRateCurve;
    [SerializeField, Range(1, 10)] private int _fireRateSteps = 1;

    private Camera _camera;
    private bool _canFire;
    private float _lastKeyPosition;

    protected override void Awake()
    {
        _camera = GetComponent<Camera>();
        _canFire = true;
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

        _canFire = false;

        float increment = _lastKeyPosition / _fireRateSteps;
        float currentStep = increment;
        float waitTime = _fireRateCurve.Evaluate(increment);
        float timer = 0f;

        while(Input.GetMouseButton(mouseButton))
        {
            timer += Time.deltaTime;

            if(timer > waitTime)
            {
                RaycastHit hit;
                if(!CheckRaycastForMatch(hitTransform, out hit)) break;

                HitResult hitResult = hitTransform.GetComponentInParent<MoveablePlane>().HandleHit(new HitData(eventArgs.FireTypeArg, hit));
                HandleHitSound(hitResult, eventArgs.FireTypeArg);

                if(hitResult == HitResult.EndOfPath) break;

                currentStep = (currentStep + increment) > _lastKeyPosition ? _lastKeyPosition : (currentStep + increment);
                waitTime = _fireRateCurve.Evaluate(currentStep);
                timer = 0f;
            }

            yield return null;
        }

        _canFire = true;
    }

    private bool CheckRaycastForMatch(Transform checkTransform, out RaycastHit hit)
    {
        if(Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit))
        {
            if(hit.transform.gameObject.layer == LayerMask.NameToLayer("MoveablePlane"))
            {
                return hit.transform == checkTransform;
            }
        }

        return false;
    }

    private void FireWeapon(WeaponFiredEventArgs eventArgs)
    {
        RaycastHit hit;
        if(Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, _range))
        {
            if(hit.transform.gameObject.layer == LayerMask.NameToLayer("MoveablePlane"))
            {
                if(_canFire)
                {
                    MoveablePlane block = hit.transform.GetComponentInParent<MoveablePlane>();
                    if(block) 
                    {
                        HandleHitSound(block.HandleHit(new HitData(eventArgs.FireTypeArg, hit)), eventArgs.FireTypeArg);
                        StartCoroutine(UpdateFireRate(eventArgs, hit.transform));
                    }
                    else Debug.LogError("No MoveableBlock component found!");
                }
            }
        } 
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
