using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitData
{
    public FireType FireTypeArg { get; private set; }
    public bool ValidTargetArg { get; private set; }

    public HitData(FireType fireType, bool validTarget)
    {
        FireTypeArg = fireType;
        ValidTargetArg = validTarget;
    }
}

public class MoveablePlane : MonoBehaviour
{
    private MoveableObject _parentMoveable;
    private Renderer _renderer;
    private Vector3Int _axes;

    private Vector3Int _currentRelativeLocation;
    private Vector3Int _totalMoveableSpaceV;
    private int _totalMoveableSpaceI;
    private float _coloringRatio;

    private void Awake()
    {
        _parentMoveable = GetComponentInParent<MoveableObject>();
        _renderer = GetComponent<Renderer>();
        _axes = Vector3Int.RoundToInt(transform.up * -1);
    }

    private void Start()
    {
        InitializeColor();
    }

    private void InitializeColor()
    {
        _totalMoveableSpaceV = new Vector3Int(Mathf.Abs(_axes.x * _parentMoveable.XMoves), Mathf.Abs(_axes.y * _parentMoveable.YMoves), Mathf.Abs(_axes.z * _parentMoveable.ZMoves));
        _totalMoveableSpaceI = _totalMoveableSpaceV.x + _totalMoveableSpaceV.y + _totalMoveableSpaceV.z;
        _coloringRatio = 1f /_totalMoveableSpaceI;

        UpdateColor(true);
    }

    public void UpdateColor(bool immediate = false)
    {
        _currentRelativeLocation = new Vector3Int(Mathf.Abs(_axes.x * _parentMoveable.CurrentRelativeLocation.x), Mathf.Abs(_axes.y * _parentMoveable.CurrentRelativeLocation.y), Mathf.Abs(_axes.z * _parentMoveable.CurrentRelativeLocation.z));

        if(_axes.x < 0)
        {
            _currentRelativeLocation.x = _totalMoveableSpaceV.x - _currentRelativeLocation.x;
        }
        if(_axes.y < 0)
        {
            _currentRelativeLocation.y = _totalMoveableSpaceV.y - _currentRelativeLocation.y;
        }
        if(_axes.z < 0)
        {
            _currentRelativeLocation.z = _totalMoveableSpaceV.z - _currentRelativeLocation.z;
        }

        if(immediate) SetAxisColors(CalcRelativeLocationRatio());
        else StartCoroutine(SmoothColorSwap(CalcRelativeLocationRatio()));
    }

    private IEnumerator SmoothColorSwap(float relativeLocationRatio)
    {
        Color currentColor = _renderer.material.color;
        Color newColor = new Color32(255, (byte)(255 * (relativeLocationRatio)), (byte)(255 * (relativeLocationRatio)), 255);
        float timer = 0;

        while(timer < _parentMoveable.MoveTime)
        {
            _renderer.material.color = Color.Lerp(currentColor, newColor, timer / _parentMoveable.MoveTime);
            timer += Time.deltaTime;
            yield return null;
        }

        _renderer.material.color = newColor;
    }

    private void SetAxisColors(float relativeLocationRatio)
    {   
        byte color = (byte)(255 * (relativeLocationRatio));

        _renderer.material.color = new Color32(255, color, color, 255);
    }

    public bool CanMove(FireType fireType)
    {
        int fireTypeMultiplier = 1;
        if(fireType == FireType.Pull) fireTypeMultiplier = -1;

        return _parentMoveable.CheckCanMove(_axes * fireTypeMultiplier, out Vector3Int v);
    }

    public HitResult HandleHit(HitData hitArgs) //This got messy, will need to clean up this process later
    {
        int fireTypeMultiplier = 1;
        if(hitArgs.FireTypeArg == FireType.Pull) fireTypeMultiplier = -1;

        if(hitArgs.ValidTargetArg) return _parentMoveable.HandleMove(_axes * fireTypeMultiplier, hitArgs.FireTypeArg, true);
        else return HitResult.Failure;
    }

    //Helpers
    public float CalcRelativeLocationRatio()
    {
        return (_currentRelativeLocation.x * _coloringRatio + _currentRelativeLocation.y * _coloringRatio + _currentRelativeLocation.z * _coloringRatio);
    }
}
