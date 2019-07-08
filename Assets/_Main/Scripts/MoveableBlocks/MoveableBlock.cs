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

    [SerializeField] private Renderer _xPos, _xNeg, _yPos, _yNeg, _zPos, _zNeg;
    [SerializeField] private float _moveTime;
    
    [HideInInspector] public bool ShowPreview = true;  
    [HideInInspector] public int XMoves, YMoves, ZMoves;
    [HideInInspector] public int XPos, YPos, ZPos;

    private bool _moving;
    private Vector3Int _numOfMoves;
    private Vector3Int _currentLocation;
    private Vector3Int _currentNormalMovingVector;
    private Vector3Int _startingLocation;

    private void Awake()
    {
        _moving = false;
        
        _numOfMoves = new Vector3Int(XMoves, YMoves, ZMoves);
        _currentLocation = new Vector3Int(XPos, YPos, ZPos);
        _currentNormalMovingVector = Vector3Int.zero;
        _startingLocation = Vector3Int.RoundToInt(_currentLocation - transform.localPosition);

        SetAllColors();
    }

    private void Update()
    {
        //CheckMovePath(Axis.X);
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
        byte posColor = (byte)(255 * ((numOfMoves - coloringLocation) / numOfMoves));
        byte negColor = (byte)(255 * ((coloringLocation) / numOfMoves));

        pos.material.color = new Color32(255, posColor, posColor, 255);
        neg.material.color = new Color32(255, negColor, negColor, 255);
    }

    private byte RoundByteColors(byte b)
    {
        if(b > 230) return 255;
        else if (b < 35) return 0;
        else return b;
    }

    public void HandleHit(HitData hitArgs)
    {
        int fireTypeMultiplier = 1;
        if(hitArgs.FireTypeArg == WeaponFiredEventArgs.FireType.Pull) fireTypeMultiplier = -1;

        _currentNormalMovingVector = Vector3Int.RoundToInt(hitArgs.Hit.transform.InverseTransformDirection(hitArgs.Hit.normal * -1f)); //The opposite of the normal is the direction we would like to push
        Vector3Int templocation = _currentLocation + _currentNormalMovingVector * fireTypeMultiplier;

        Axis axis = Axis.NONE;

        if(_currentNormalMovingVector.x != 0  && templocation.x >= 0 && templocation.x <= _numOfMoves.x && CheckMovePath(Axis.X))
        {
            _currentLocation.x = templocation.x;
            axis = Axis.X;
        }
        else if(_currentNormalMovingVector.y != 0 && templocation.y >= 0 && templocation.y <= _numOfMoves.y && CheckMovePath(Axis.Y))
        {
            _currentLocation.y = templocation.y;
            axis = Axis.Y;
        }
        else if(_currentNormalMovingVector.z != 0 && templocation.z >= 0 && templocation.z <= _numOfMoves.z && CheckMovePath(Axis.Z))
        {
            _currentLocation.z = templocation.z;
            axis = Axis.Z;
        }

        if(axis != Axis.NONE) StartCoroutine(MovePiece(transform.localPosition, _currentLocation - _startingLocation, axis));
    }

    private bool CheckMovePath(Axis axis)
    {
        Vector3 raycastStartAdjustment = Vector3.zero;
        Vector3 raycastDirection = Vector3.zero;
        float raycastLength = 0;
        
        switch(axis)
        {
            case Axis.X:
                raycastStartAdjustment += new Vector3(transform.localScale.x / 2 * _currentNormalMovingVector.x, 0, 0);
                raycastDirection += transform.forward * _currentNormalMovingVector.x;
                raycastLength = transform.localScale.x;
                break;
            case Axis.Y:
                raycastStartAdjustment += new Vector3(0, transform.localScale.y / 2 * _currentNormalMovingVector.y, 0);
                raycastDirection += transform.up * _currentNormalMovingVector.y;
                raycastLength = transform.localScale.y;
                break;
            case Axis.Z:
                raycastStartAdjustment += new Vector3(0, 0, transform.localScale.z / 2 * _currentNormalMovingVector.z);
                raycastDirection += transform.right * _currentNormalMovingVector.z;
                raycastLength = transform.localScale.z;
                break;
        }
        Debug.DrawRay(transform.position + raycastStartAdjustment, raycastDirection, Color.magenta, raycastLength);
        if(Physics.Raycast(transform.position + raycastStartAdjustment, raycastDirection, out RaycastHit hit, raycastLength, LayerMask.NameToLayer("Player")))
        {
            return false;
        }
        
        return true;
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
        SetAxisColors(axis, endingLocation);
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
