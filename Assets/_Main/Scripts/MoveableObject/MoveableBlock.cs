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
    [SerializeField] private float _moveTime = .1f;
    [SerializeField] private float _verticalLaunchSpeed = 17f;
    [SerializeField] private float _horizontalLaunchSpeed = 25f;
    [Range(1, 10)] public int MoveDistance = 2;

    [HideInInspector] public bool ShowPreview = true;  
    [HideInInspector] public int XMoves, YMoves, ZMoves;
    [HideInInspector] public int XPos, YPos, ZPos;

    private bool _moving;
    private float _moveDistance;
    private Vector3Int _numOfMoves;
    private Vector3Int _currentRelativeLocation;
    private Vector3 _currentLocation;
    private Vector3 _currentNormalMovingVector;
    private Vector3Int _startingLocation;

    private void Awake()
    {
        _moving = false;
        
        _numOfMoves = new Vector3Int(XMoves, YMoves, ZMoves);
        _moveDistance = MoveDistance;
        _currentRelativeLocation = new Vector3Int(XPos, YPos, ZPos);
        _currentLocation = new Vector3(XPos, YPos, ZPos);
        _currentNormalMovingVector = Vector3.zero;
        _startingLocation = Vector3Int.RoundToInt(_currentRelativeLocation - transform.parent.localPosition);

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
                SetAxisColors(_xPos, _xNeg, _currentRelativeLocation.x, _numOfMoves.x);
                break;
            case Axis.Y:
                SetAxisColors(_yPos, _yNeg, _currentRelativeLocation.y, _numOfMoves.y);
                break;
            case Axis.Z:
                SetAxisColors(_zPos, _zNeg, _currentRelativeLocation.z, _numOfMoves.z);
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
        byte posColor = (byte)(255 * (numOfMoves - coloringLocation) / numOfMoves);
        byte negColor = (byte)(255 * (coloringLocation) / numOfMoves);

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

        _currentNormalMovingVector = hitArgs.Hit.transform.InverseTransformDirection(hitArgs.Hit.normal * -1f);
        Vector3Int templocation = _currentRelativeLocation + Vector3Int.RoundToInt(new Vector3(_currentNormalMovingVector.x * fireTypeMultiplier, 
                                                                                                _currentNormalMovingVector.y * fireTypeMultiplier, 
                                                                                                 _currentNormalMovingVector.z * fireTypeMultiplier));
        Axis axis = Axis.NONE;

        if(_currentNormalMovingVector.x != 0  && templocation.x >= 0 && templocation.x <= _numOfMoves.x && CheckMovePath(Axis.X, _currentNormalMovingVector * fireTypeMultiplier))
        {
            _currentRelativeLocation.x = templocation.x;
            _currentLocation.x = (_currentRelativeLocation.x - _startingLocation.x) * MoveDistance / transform.parent.localScale.x;
            axis = Axis.X;
        }
        else if(_currentNormalMovingVector.y != 0 && templocation.y >= 0 && templocation.y <= _numOfMoves.y && CheckMovePath(Axis.Y, _currentNormalMovingVector* fireTypeMultiplier))
        {
            _currentRelativeLocation.y = templocation.y;
            _currentLocation.y = (_currentRelativeLocation.y - _startingLocation.y) * MoveDistance / transform.parent.localScale.y;
            axis = Axis.Y;
        }
        else if(_currentNormalMovingVector.z != 0 && templocation.z >= 0 && templocation.z <= _numOfMoves.z && CheckMovePath(Axis.Z, _currentNormalMovingVector * fireTypeMultiplier))
        {
            _currentRelativeLocation.z = templocation.z;
            _currentLocation.z = (_currentRelativeLocation.z - _startingLocation.x) * MoveDistance / transform.parent.localScale.z;
            axis = Axis.Z;
        }

        if(axis != Axis.NONE) 
        {
            if(hitArgs.FireTypeArg == WeaponFiredEventArgs.FireType.Pull)
            {
                AudioManager.Instance.PlayPullSound();
            }
            else
            {
                AudioManager.Instance.PlayPushSound();
            }

            StartCoroutine(MovePiece(transform.parent.localPosition, _currentLocation, axis));
        }
        else
        {
            AudioManager.Instance.PlayDudSound();
        }
    }

    private bool CheckMovePath(Axis axis, Vector3 movingVector) //Makes sure the path the cube is going to travel is clear
    {
        Vector3 raycastStartAdjustment = Vector3.zero;
        Vector3 raycastDirection = Vector3.zero;
        float raycastLength = 0;
        
        switch(axis)
        {
            case Axis.X:
                raycastStartAdjustment += new Vector3(transform.localScale.x / 2 * movingVector.x - Mathf.Sign(movingVector.x) * .05f, transform.localScale.y / 2, 0);
                raycastDirection += transform.right * movingVector.x;
                raycastLength = _moveDistance;
                break;
            case Axis.Y:
                raycastStartAdjustment += new Vector3(0, transform.localScale.y / 2 * movingVector.y + transform.localScale.y / 2  - Mathf.Sign(movingVector.y) * .05f, 0);
                raycastDirection += transform.up * movingVector.y;
                raycastLength = _moveDistance;
                break;
            case Axis.Z:
                raycastStartAdjustment += new Vector3(0, transform.localScale.y / 2, transform.localScale.z / 2 * movingVector.z  - Mathf.Sign(movingVector.z) * .05f);
                raycastDirection += transform.forward * movingVector.z;
                raycastLength = _moveDistance;
                break;
        }
        
        Debug.DrawRay(transform.position + raycastStartAdjustment, raycastDirection * raycastLength, Color.magenta, 1);
        if(Physics.Raycast(transform.position + raycastStartAdjustment, raycastDirection, out RaycastHit hit, raycastLength)) return false;

        return true;
    }

    private IEnumerator MovePiece(Vector3 startingLocation, Vector3 endingLocation, Axis axis) //Controls piece movement and smooth color changes
    {
        float timer = 0f;

        _moving = true;

        while(timer < _moveTime)
        {
            transform.parent.localPosition = Vector3.Lerp(startingLocation, endingLocation, timer / _moveTime);
            SetAxisColors(axis, new Vector3(transform.parent.localPosition.x / _moveDistance, transform.parent.localPosition.y / _moveDistance, transform.parent.localPosition.z / _moveDistance)); //I hate this line
            timer += Time.deltaTime;
            yield return null;
        }

        transform.parent.localPosition = endingLocation;
        SetAxisColors(axis, _currentRelativeLocation);
        _moving = false;
        _currentNormalMovingVector = Vector3Int.zero;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && _moving) //Fires launch event if the player is hit
        {
            _moving = false;
            EventManager.TriggerEvent(new PlayerLaunchEvent(_currentNormalMovingVector, _verticalLaunchSpeed, _horizontalLaunchSpeed));
        }
    }
}
