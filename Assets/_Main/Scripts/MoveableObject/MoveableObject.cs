using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Range(1, 10)] public int MoveDistance = 2;

    [HideInInspector] public bool ShowPreview = true;  
    [HideInInspector] public int XMoves, YMoves, ZMoves;
    [HideInInspector] public int XPos, YPos, ZPos;

    private bool _moving;
    private float _moveDistance;
    private Vector3Int _numOfMoves;
    private Vector3Int _currentRelativeLocation; //Our current location, relative to the positions and moves we have set
    private Vector3Int _currentNormalMovingVector;
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

        _boundingBox = GetComponentInChildren<BoundingBox>();
        if(_boundingBox == null) Debug.LogError("No bounding box as child to movable object!");
        
        _moveableStructure = GetComponentInChildren<MoveableStructure>();
        if(_moveableStructure == null) Debug.LogError("No moveable structure as child to movable object!");
    }

    public void HandleMove(Vector3Int movingVector, WeaponFiredEventArgs.FireType fireType)
    {
        Vector3Int tempLocation = _currentRelativeLocation + movingVector; //Where we are trying to move
        Vector3Int movingVectorAdjustment = Vector3Int.zero; //The adjustment we will move to, determined by where we actually can move

        bool movePiece = false;
        if(Mathf.Abs(movingVector.x) > 0 && tempLocation.x >= 0 && tempLocation.x <= _numOfMoves.x)
        {
            movePiece = true;
            movingVectorAdjustment.x += movingVector.x;
        }
        if(Mathf.Abs(movingVector.y) > 0 && tempLocation.y >= 0 && tempLocation.y <= _numOfMoves.y)
        {
            movePiece = true;
            movingVectorAdjustment.y += movingVector.y;
        }
        if(Mathf.Abs(movingVector.z) > 0 && tempLocation.z >= 0 && tempLocation.z <= _numOfMoves.z)
        {
            movePiece = true;
            movingVectorAdjustment.z += movingVector.z;
        }

        if(CheckMovePathClear(movingVectorAdjustment) && movePiece)
        {
            _currentNormalMovingVector = movingVector;
            _currentRelativeLocation += movingVectorAdjustment;
            StartCoroutine(Move());
            _moveableStructure.UpdatePlaneColors();

            if(fireType == WeaponFiredEventArgs.FireType.Pull)
            {
                AudioManager.Instance.PlayPullSound();
            }
            else
            {
                AudioManager.Instance.PlayPushSound();
            }
        }
        else
        {
            AudioManager.Instance.PlayDudSound();
        }

        _moveableStructure.ReturnLayer();
    }

    private bool CheckMovePathClear(Vector3 movingVector) //Makes sure the path the cube is going to travel is clear, could use improvement but good enough for now
    {
        Vector3 adjustmentVector = new Vector3(movingVector.x * _boundingBox.transform.localScale.x / 2, movingVector.y * _boundingBox.transform.localScale.y / 2, movingVector.z * _boundingBox.transform.localScale.z / 2);

        _moveableStructure.HideChildrenFromRaycast();

        Debug.DrawRay(_boundingBox.transform.position, movingVector * _moveDistance + adjustmentVector, Color.magenta, 1);
        if(Physics.SphereCast(_boundingBox.transform.position, _boundingBox.SpherecastRadius, movingVector, out RaycastHit hit, _moveDistance * movingVector.magnitude + adjustmentVector.magnitude - _boundingBox.SpherecastRadius)) return false;
        return true;
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
                if(player)
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
            EventManager.TriggerEvent(new PlayerLaunchEvent(_currentNormalMovingVector * -1, _verticalLaunchSpeed, _horizontalLaunchSpeed));
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
