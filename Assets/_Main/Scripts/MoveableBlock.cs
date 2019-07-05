using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitData
{
    public WeaponFiredEventArgs.FireType FireTypeArg { get; private set; }
    public RaycastHit Hit { get; private set; }

    public HitData(WeaponFiredEventArgs.FireType fireType, RaycastHit hit)
    {
        FireTypeArg = fireType;
        Hit = hit;
    }
}

public struct PlayerLaunchEvent : IGameEvent
{
    public Vector3 LaunchVector { get; private set; }

    public PlayerLaunchEvent(Vector3 launchVector)
    {
        LaunchVector = launchVector;
    }
}

public class MoveableBlock : MonoBehaviour
{
    private enum Axis
    {
        NONE,
        X,
        Y,
        Z
    }

    [SerializeField] private Renderer _xPos;
    [SerializeField] private Renderer _xNeg;
    [SerializeField] private Renderer _yPos;
    [SerializeField] private Renderer _yNeg;
    [SerializeField] private Renderer _zPos;
    [SerializeField] private Renderer _zNeg;

    [SerializeField] private Vector3Int _numOfMoves;
    [SerializeField] private Vector3Int _currentLocation;
    [SerializeField] private float _moveTime;

    private bool _moving;
    private Vector3Int _currentNormalMovingVector;
    private Vector3Int _startingLocation;

    private void Awake()
    {
        _moving = false;
        _currentNormalMovingVector = Vector3Int.zero;
        _startingLocation = Vector3Int.RoundToInt(_currentLocation - transform.localPosition);

        SetAllColors();
    }

    private void SetAllColors()
    {
        if(_numOfMoves.x == 0)
        {
            _xPos.material.color = Color.black;
            _xNeg.material.color = Color.black;
        }
        else
        {
            SetAxisColors(Axis.X);
        }
        if(_numOfMoves.y == 0)
        {
            _yPos.material.color = Color.black;
            _yNeg.material.color = Color.black;
        }
        else
        {
            SetAxisColors(Axis.Y);
        }
        if(_numOfMoves.z == 0)
        {
            _zPos.material.color = Color.black;
            _zNeg.material.color = Color.black;
        }
        else
        {
            SetAxisColors(Axis.Z);
        }
    }

    private void SetAxisColors(Axis axis)
    {
        switch(axis)
        {
            case Axis.X:
                SetAxisColors(_xPos, _xNeg, _currentLocation.x, _numOfMoves.x);
                break;
            case Axis.Y:
                SetAxisColors(_yPos, _yNeg, _currentLocation.y, _numOfMoves.y);
                break;
            case Axis.Z:
                SetAxisColors(_zPos, _zNeg, _currentLocation.z, _numOfMoves.z);
                break;
        }
    }

    private void SetAxisColors(Axis axis, Vector3 currentLocation)
    {
        switch(axis)
        {
            case Axis.X:
                SetAxisColors(_xPos, _xNeg, currentLocation.x, _numOfMoves.x);
                break;
            case Axis.Y:
                SetAxisColors(_yPos, _yNeg, currentLocation.y, _numOfMoves.y);
                break;
            case Axis.Z:
                SetAxisColors(_zPos, _zNeg, currentLocation.z, _numOfMoves.z);
                break;
        }
    }

    private void SetAxisColors(Renderer pos, Renderer neg, float coloringLocation, int numOfMoves)
    {   
        pos.material.color = new Color32(255, (byte)(255 * ((numOfMoves - coloringLocation) / numOfMoves)), (byte)(255 * ((numOfMoves - coloringLocation) / numOfMoves)), 255);
        neg.material.color = new Color32(255, (byte)(255 * ((coloringLocation) / numOfMoves)), (byte)(255 * ((coloringLocation) / numOfMoves)), 255);
    }

    public void HandleHit(HitData hitArgs)
    {
        int fireTypeMultiplier = 1;
        if(hitArgs.FireTypeArg == WeaponFiredEventArgs.FireType.Pull) fireTypeMultiplier = -1;

        _currentNormalMovingVector = Vector3Int.RoundToInt(hitArgs.Hit.transform.InverseTransformDirection(hitArgs.Hit.normal * -1f)); //The opposite of the normal is the direction we would like to push
        Vector3Int templocation = _currentLocation + _currentNormalMovingVector * fireTypeMultiplier;

        Axis axis = Axis.NONE;

        if(_currentNormalMovingVector.x != 0  && templocation.x >= 0 && templocation.x <= _numOfMoves.x)
        {
            _currentLocation.x = templocation.x;
            axis = Axis.X;
        }
        else if(_currentNormalMovingVector.y != 0 && templocation.y >= 0 && templocation.y <= _numOfMoves.y)
        {
            _currentLocation.y = templocation.y;
            axis = Axis.Y;
        }
        else if(_currentNormalMovingVector.z != 0 && templocation.z >= 0 && templocation.z <= _numOfMoves.z)
        {
            _currentLocation.z = templocation.z;
            axis = Axis.Z;
        }

        if(axis != Axis.NONE) StartCoroutine(MovePiece(transform.localPosition, _currentLocation - _startingLocation, axis));
    }

    private IEnumerator MovePiece(Vector3 startingLocation, Vector3Int endingLocation, Axis axis)
    {
        float timer = 0f;

        _moving = true;
        Vector3 endingLocationF = new Vector3(endingLocation.x, endingLocation.y, endingLocation.z);

        while(timer < _moveTime)
        {
            transform.localPosition = Vector3.Lerp(startingLocation, endingLocationF, timer / _moveTime);
            SetAxisColors(axis, transform.localPosition - _startingLocation);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = endingLocation;
        _moving = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && _moving)
        {
            _moving = false;
            EventManager.TriggerEvent(new PlayerLaunchEvent(_currentNormalMovingVector));
        }
    }
}
