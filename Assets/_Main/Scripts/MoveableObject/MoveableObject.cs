using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitResult
{
    Success,
    Failure,
    EndOfPath
}

public struct PlayerLaunchEvent : IGameEvent
{
    public Vector3 LaunchVector { get; private set; }
    public float VerticalLaunchSpeed { get; private set; }
    public float HorizontalLaunchSpeed { get; private set; }

    public PlayerLaunchEvent(Vector3 launchVector, float verticalLaunchSpeed, float horizontalLaunchSpeed)
    {
        LaunchVector = launchVector;
        VerticalLaunchSpeed = verticalLaunchSpeed;
        HorizontalLaunchSpeed = horizontalLaunchSpeed;
    }
}

public class MoveableObject : MonoBehaviour
{
    [SerializeField] private float _moveTime = .1f;
    [SerializeField] private float _verticalLaunchSpeed = 17f;
    [SerializeField] private float _horizontalLaunchSpeed = 25f;
    [SerializeField] private float _diagonalVerticalLaunchSpeed = 12f;
    [SerializeField] private float _diagonalHorizontalLaunchSpeed = 20f;
    [Range(1, 10)] public int MoveDistance = 2;

    [HideInInspector] public bool ShowPreview = true;  
    [HideInInspector] public int XMoves, YMoves, ZMoves;
    [HideInInspector] public int XPos, YPos, ZPos;

    private bool _moving;
    private float _moveDistance;
    private Vector3Int _numOfMoves;
    private Vector3Int _currentRelativeLocation; //Our current location, relative to the positions and moves we have set
    private Vector3Int _currentNormalMovingVector;
    private Vector3Int _endOfPath;
    private BoundingBox _boundingBox;
    private MoveableStructure _moveableStructure;

    public bool IsMoving { get { return _moving; } }
    public float MoveTime { get { return _moveTime; } }
    public Vector3Int CurrentRelativeLocation { get { return _currentRelativeLocation; } }

    private void Awake()
    {
        _moveDistance = MoveDistance;

        _moving = false;
        _numOfMoves = new Vector3Int(XMoves, YMoves, ZMoves);
        _currentRelativeLocation = new Vector3Int(XPos, YPos, ZPos);
        _currentNormalMovingVector = Vector3Int.zero;
        _endOfPath = Vector3Int.zero;

        if(XPos == XMoves || XPos == 0) _endOfPath.x = 1;
        if(YPos == YMoves || YPos == 0) _endOfPath.y = 1;
        if(ZPos == ZMoves || ZPos == 0) _endOfPath.z = 1;

        _boundingBox = GetComponentInChildren<BoundingBox>();
        if(_boundingBox == null) Debug.LogError("No bounding box as child to movable object!");
        
        _moveableStructure = GetComponentInChildren<MoveableStructure>();
        if(_moveableStructure == null) Debug.LogError("No moveable structure as child to movable object!");
    }

    public bool CheckCanMove(Vector3Int movingVector, out Vector3Int movingVectorAdjustment, bool overrideCheck = false)
    {
        Vector3Int tempLocation = _currentRelativeLocation + movingVector; //Where we are trying to move
        movingVectorAdjustment = Vector3Int.zero; //The adjustment we will move to, determined by where we actually can move

        bool movePiece = false;
        if(Mathf.Abs(movingVector.x) > 0 && tempLocation.x >= 0 && tempLocation.x <= _numOfMoves.x)
        {
            if(tempLocation.x == _numOfMoves.x || tempLocation.x == 0) _endOfPath.x = 1;
            else _endOfPath.x = 0;

            movePiece = true;
            movingVectorAdjustment.x += movingVector.x;
        }
        if(Mathf.Abs(movingVector.y) > 0 && tempLocation.y >= 0 && tempLocation.y <= _numOfMoves.y)
        {
            if(tempLocation.y == _numOfMoves.y || tempLocation.y == 0) _endOfPath.y = 1;
            else _endOfPath.y = 0;

            movePiece = true;
            movingVectorAdjustment.y += movingVector.y;
        }
        if(Mathf.Abs(movingVector.z) > 0 && tempLocation.z >= 0 && tempLocation.z <= _numOfMoves.z)
        {
            if(tempLocation.z == _numOfMoves.z || tempLocation.z == 0) _endOfPath.z = 1;
            else _endOfPath.z = 0;

            movePiece = true;
            movingVectorAdjustment.z += movingVector.z;
        }

        if(overrideCheck) return true;

        return CheckMovePathClear(movingVectorAdjustment) && movePiece;
    }

    private bool CheckMovePathClear(Vector3 movingVector) //Makes sure the path the cube is going to travel is clear, could use improvement but good enough for now
    {
        Vector3 adjustmentVector = new Vector3(movingVector.x * _boundingBox.transform.localScale.x / 2, movingVector.y * _boundingBox.transform.localScale.y / 2, movingVector.z * _boundingBox.transform.localScale.z / 2);
        float adjustmentVectorMagnitude = adjustmentVector.magnitude; 
        
        if(Mathf.Abs(movingVector.x) + Mathf.Abs(movingVector.y) + Mathf.Abs(movingVector.z) > 1) // Multiplier for diagonal movement, maybe just do 2 raycasts instead??
        {
            adjustmentVectorMagnitude *= .8f;
        }

        _moveableStructure.HideChildrenFromRaycast(); //I have no idea if changing the layer of a bunch of objects for a single raycast is terrible or not

        //Debug.DrawRay(_boundingBox.transform.position, movingVector * _moveDistance + adjustmentVector, Color.magenta, 1);
        if(Physics.SphereCast(_boundingBox.transform.position, _boundingBox.SpherecastRadius, movingVector, out RaycastHit hit, _moveDistance * movingVector.magnitude + adjustmentVectorMagnitude - _boundingBox.SpherecastRadius))
        {
            _moveableStructure.ReturnLayer();
            return false;
        }

        _moveableStructure.ReturnLayer();
        return true;
    }

    public HitResult HandleMove(Vector3Int movingVector, FireType fireType, bool overrideCheck = false)
    {
        Vector3Int movingVectorAdjustment;
        bool move = CheckCanMove(movingVector, out movingVectorAdjustment, overrideCheck);
        
        if(CheckCanMove(movingVector, out movingVectorAdjustment))
        {
            _currentNormalMovingVector = movingVector;
            _currentRelativeLocation += movingVectorAdjustment;
            StartCoroutine(Move());
            _moveableStructure.UpdatePlaneColors();

            if((_endOfPath * movingVectorAdjustment).magnitude >= movingVectorAdjustment.magnitude) //If endofpath and movingvectoradjustment both have 1's (true)
            {
                return HitResult.EndOfPath;
            }
            else
            {
                return HitResult.Success;
            }
        }
        else
        {
            return HitResult.Failure;
        }
    }

    private IEnumerator Move() //Controls piece movement and smooth color changes
    {
        float timer = 0f;
        _moving = true;

        Vector3 startingLocation = transform.localPosition;
        Vector3 endingLocation = new Vector3(_currentRelativeLocation.x * _moveDistance, _currentRelativeLocation.y * _moveDistance, _currentRelativeLocation.z * _moveDistance);

        while(timer < _moveTime)
        {
            if(_currentNormalMovingVector.y > 0)//TEMPORARY UNIL I FIND A BETTER SOLUTION, I HATE THIS
            {
                UnityStandardAssets.Characters.FirstPerson.FirstPersonController player = GetComponentInChildren<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
                if(player && !Input.GetKey(KeyCode.LeftShift))
                {
                    player.transform.parent = null;
                }
            }

            transform.localPosition = Vector3.Lerp(startingLocation, endingLocation, timer / _moveTime);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = endingLocation;
        _currentNormalMovingVector = Vector3Int.zero;
        _moving = false;
    }

    public void LaunchPlayer()
    {
        if(_moving)
        {
            _moving = false;
            if(_currentNormalMovingVector.magnitude <= 1) EventManager.TriggerEvent(new PlayerLaunchEvent(_currentNormalMovingVector * -1, _verticalLaunchSpeed, _horizontalLaunchSpeed));
            else EventManager.TriggerEvent(new PlayerLaunchEvent(_currentNormalMovingVector * -1, _diagonalVerticalLaunchSpeed, _diagonalHorizontalLaunchSpeed));
        }
    }

    //Helpers
    private bool GreaterMagnitudeThanFloat(float a, float b)
    {
        if(Mathf.Abs(a - b) > .01f) return true;
        return false;
    }

    private float GetSmallest(List<float> floats)
    {
        float smallest = floats[0];

        for(int i = 1; i < floats.Count; i++)
        {
            if(floats[i] < smallest)
            {
                smallest = floats[i];
            }
        }

        return smallest;
    }
}
