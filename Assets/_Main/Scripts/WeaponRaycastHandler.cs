using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class WeaponRaycastHandler : GameEventUserObject
{
    [SerializeField] private float _range = 50f;

    private Camera _camera;

    protected override void Awake()
    {
        _camera = GetComponent<Camera>();

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

    private void FireWeapon(WeaponFiredEventArgs eventArgs)
    {
        RaycastHit hit;
        if(Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, _range))
        {
            if(hit.transform.gameObject.layer == LayerMask.NameToLayer("MoveableObject"))
            {
                MoveableBlock block = hit.transform.GetComponentInParent<MoveableBlock>();
                if(block) block.HandleHit(new HitData(eventArgs.FireTypeArg, hit));
                else Debug.LogError("No MoveableBlock component found!");
            }
            else
            {
                AudioManager.Instance.PlayDudSound();
            } 
        } 
    }
}
