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
            case WeaponFiredEventArgs.FireType.Push:
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

        RaycastHit hit;

        while(Input.GetMouseButton(mouseButton) && CheckRaycastForMatch(hitTransform, out hit))
        {
            timer += Time.deltaTime;

            if(timer > waitTime)
            {
                hitTransform.GetComponentInParent<MoveableBlock>().HandleHit(new HitData(eventArgs.FireTypeArg, hit));

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
        if(Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, _range))
        {
            return hit.transform == checkTransform;
        }

        return false;
    }

    private void FireWeapon(WeaponFiredEventArgs eventArgs)
    {
        RaycastHit hit;
        if(Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, _range))
        {
            if(hit.transform.gameObject.layer == LayerMask.NameToLayer("MoveableObject") && _canFire)
            {
                MoveableBlock block = hit.transform.GetComponentInParent<MoveableBlock>();
                if(block) 
                {
                    block.HandleHit(new HitData(eventArgs.FireTypeArg, hit));
                    StartCoroutine(UpdateFireRate(eventArgs, hit.transform));
                }
                else Debug.LogError("No MoveableBlock component found!");
            }
            else
            {
                AudioManager.Instance.PlayDudSound();
            } 
        } 
    }
}
