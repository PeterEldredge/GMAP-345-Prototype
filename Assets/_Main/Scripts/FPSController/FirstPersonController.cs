using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

public struct WeaponFiredEventArgs : IGameEvent
{
    public enum FireType
    {
        Push,
        Pull
    }

    public FireType FireTypeArg { get; private set; }

    public WeaponFiredEventArgs(FireType fireType)
    {
        FireTypeArg = fireType;
    }
}

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : GameEventUserObject
    {
        //Movement
        [SerializeField] private float _stickToGroundForce = 10f;
        [SerializeField] private float _aimMoveSpeed = 3f;  
        [SerializeField] private float _walkSpeed = 5f;
        [SerializeField] private float _runSpeed = 10f;
        [SerializeField] [Range(0f, 1f)] private float _runstepLenghten = .7f;
        private bool _isWalking = false;
        private float _moveAngle; //The angle which the player is moving
        private float _facingAngle; //The angle which the player is facing
        private float _speed; //Current Movement Speed
        private float _speedInFacingDirection;
        private Vector3 _moveVector = Vector3.zero;

        //Jump
        [SerializeField] private float _jumpSpeed = 10f;
        private bool _jump;
        private bool _jumping;
        private bool _previouslyGrounded;

        //Launching (Needs to be moved to launch event)
        [SerializeField] private float _horrizontalLaunchAirDampening = 6f;
        [SerializeField] private float _horrizontalLaunchGroundDampening = 10f;
        [SerializeField] private float _launchControl = 5f;
        [SerializeField] private float _verticalLaunchSpeed = 17f;
        [SerializeField] private float _horrizontalLaunchSpeed = 25f;
        [SerializeField] private float _launchTime = 3f;
        [SerializeField] private float _verticalLaunchGravityMultiplier = 1.5f;
        [SerializeField] private float _horrizontalLaunchGravityMultiplier = 1f;
        [SerializeField] private float _gravityMultiplier = 2f;
        private bool _launch;
        private bool _isLaunchingVertiaclly;
        private bool _isLaunchingHorrizontally;
        private Vector3 _launchVector;

        //Mouse Functionality
        [SerializeField] private MouseLook _mouseLook;
        private Vector2 _input;

        //Headbob
        [SerializeField] private bool m_UseHeadBob = true;
        [SerializeField] private CurveControlledBob _headBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob _jumpBob = new LerpControlledBob();

        //Steps
        [SerializeField] private float _stepInterval = 5;
        private float _stepCycle;
        private float _nextStep;

        //Character Animations/Weapons
        [SerializeField] private Camera _characterCamera;
        [SerializeField] private Animator _handAnimator;
        [SerializeField] private Animator _weaponAnimator;
        [SerializeField] private float _zoomedFovChange;
        [SerializeField] private float _zoomTime;
        private bool _isAiming;
        private bool _isZoomed;
        private float _startingFov;
        private float _startingCharacterFOV;

        //Audio
        [SerializeField] private AudioClip[] _footstepSounds = null;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip _jumpSound = null;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip _landSound = null;           // the sound played when character touches back on ground.
        private AudioSource _audioSource;

        //Not Organized members
        private Camera _camera;
        private Vector3 _originalCameraPosition;
        private CharacterController _characterController;
        private CollisionFlags _collisionFlags;

        // Use this for initialization
        private void Start()
        {
            _camera = Camera.main;
            _originalCameraPosition = _camera.transform.localPosition;

            _characterController = GetComponent<CharacterController>();
            _audioSource = GetComponent<AudioSource>();

            _headBob.Setup(_camera, _stepInterval);

            _speed = 0f;
            _stepCycle = 0f;
            _nextStep = _stepCycle/2f;

			_mouseLook.Init(transform , _camera.transform);

            _jumping = false;
            _isZoomed = false;
            _launch = false;
            _isLaunchingVertiaclly = false;
            _isLaunchingHorrizontally = false;

            _launchVector = Vector3.zero;

            _startingFov = _camera.fieldOfView;
            _startingCharacterFOV = _characterCamera.fieldOfView;

            LockCursor();
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public override void Subscribe()
        {
            EventManager.AddListener<PlayerLaunchEvent>(Launch);
        }

        public override void Unsubscribe()
        {
            EventManager.RemoveListener<PlayerLaunchEvent>(Launch);
        }

        private void Launch(PlayerLaunchEvent eventArgs)
        {
            _launch = true;
            if(!CompareToZero(eventArgs.LaunchVector.x) || !CompareToZero(eventArgs.LaunchVector.x)) _launchVector = eventArgs.LaunchVector * _horrizontalLaunchSpeed * -1f;
            else _launchVector = eventArgs.LaunchVector * _verticalLaunchSpeed * -1f;
        }

        private bool CompareToZero(float f, float error = .01f)
        {
            return Mathf.Abs(f) < error;
        }

        private IEnumerator LockInput(float lockTime)
        {
            float timer = 0f;
            
            while (timer < lockTime)
            {
                yield return null;
            }
        }

        private void Update()
        {
            UpdateMembers();

            RotateView();

            if (!_previouslyGrounded && _characterController.isGrounded && !_launch)
            {
                StartCoroutine(_jumpBob.DoBobCycle());
                PlayLandingSound();
                _moveVector.y = 0f;
                _jumping = false;
            }
            if (!_characterController.isGrounded && !_jumping && !_isLaunchingVertiaclly && !_isLaunchingHorrizontally && _previouslyGrounded)
            {
                _moveVector.y = 0f;
            }

            _previouslyGrounded = _characterController.isGrounded;

            if(_isLaunchingHorrizontally) GetHorrizontalLaunchInput();
            else GetInput();

            if(_launch)
            {
                _launch = false;
                _isLaunchingVertiaclly = !CompareToZero(_launchVector.y);
                _isLaunchingHorrizontally = !CompareToZero(_launchVector.x) || !CompareToZero(_launchVector.z);
                _moveVector = _launchVector;
            }
            else if (_characterController.isGrounded)
            {
                _moveVector.y = -_stickToGroundForce;

                _isLaunchingVertiaclly = false;

                if (_jump)
                {
                    _moveVector.y = _jumpSpeed;
                    PlayJumpSound();
                    _jump = false;
                    _jumping = true;
                }
            }
            else
            {
                if(_isLaunchingHorrizontally) _moveVector += Physics.gravity*_gravityMultiplier*Time.deltaTime;
                else if(_isLaunchingVertiaclly) _moveVector += Physics.gravity*_gravityMultiplier*Time.deltaTime;
                else _moveVector += Physics.gravity*_gravityMultiplier*Time.deltaTime;
            }
            _collisionFlags = _characterController.Move(_moveVector*Time.deltaTime);

            Fire(Input.GetMouseButtonDown(0), Input.GetMouseButtonDown(1));
            ProgressStepCycle();
            UpdateCameraPosition();
            UpdateAnimations();
        }

        private void UpdateMembers()
        {
            //Get Jump Input
            if (!_jump && _characterController.isGrounded)
            {
                _jump = Input.GetButtonDown("Jump");
            }
        }

        private void PlayLandingSound()
        {
            _audioSource.clip = _landSound;
            _audioSource.Play();
            _nextStep = _stepCycle + .5f;
        }


        private void PlayJumpSound()
        {
            _audioSource.clip = _jumpSound;
            _audioSource.Play();
        }
        

        private void ProgressStepCycle()
        {
            if (_characterController.velocity.sqrMagnitude > 0 && (_input.x != 0 || _input.y != 0))
            {
                _stepCycle += (_characterController.velocity.magnitude + (_speed*(_isWalking ? 1f : _runstepLenghten)))*
                             Time.deltaTime;
            }

            if (!(_stepCycle > _nextStep))
            {
                return;
            }

            _nextStep = _stepCycle + _stepInterval;

            if (!_isLaunchingHorrizontally) PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!_characterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, _footstepSounds.Length);
            _audioSource.clip = _footstepSounds[n];
            _audioSource.PlayOneShot(_audioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            _footstepSounds[n] = _footstepSounds[0];
            _footstepSounds[0] = _audioSource.clip;
        }


        private void UpdateCameraPosition()
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (_characterController.velocity.magnitude > 0 && _characterController.isGrounded && !_isLaunchingHorrizontally)
            {
                _camera.transform.localPosition =
                    _headBob.DoHeadBob(_characterController.velocity.magnitude +
                                      (_speed*(_isWalking ? 1f : _runstepLenghten)));
                newCameraPosition = _camera.transform.localPosition;
                newCameraPosition.y = _camera.transform.localPosition.y - _jumpBob.Offset();
            }
            else
            {
                newCameraPosition = _camera.transform.localPosition;
                newCameraPosition.y = _originalCameraPosition.y - _jumpBob.Offset();
            }
            _camera.transform.localPosition = newCameraPosition;
        }

        private void UpdateAnimations()
        {
            //Should be moved upwards in the pipeline so that these members may be used by the whole class
            _facingAngle = transform.rotation.eulerAngles.y;
            if(_facingAngle < 0) _facingAngle += 360;

            _moveAngle = Mathf.Atan(_moveVector.x / _moveVector.z) * Mathf.Rad2Deg;
            if(Mathf.Sign(_moveVector.z) < 0) _moveAngle += 180;
            else if(Mathf.Sign(_moveVector.x) < 0) _moveAngle += 360;

            _speedInFacingDirection = _speed * Mathf.Cos((_facingAngle - _moveAngle) * Mathf.Deg2Rad);

            if(_speedInFacingDirection > 7 && _characterController.isGrounded) _handAnimator.SetBool("IsRunning", true);
            else _handAnimator.SetBool("IsRunning", false);
        }


        private void GetInput()
        {
            // Read input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            bool waswalking = _isWalking;

            _isWalking = !Input.GetKey(KeyCode.LeftShift);

            _speed = _isWalking ? _walkSpeed : _runSpeed;
            //}

            _input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (_input.sqrMagnitude > 1)
            {
                _input.Normalize();
            }

            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward*_input.y + transform.right*_input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, _characterController.radius / 2, Vector3.down, out hitInfo,
                               _characterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            _moveVector.x = desiredMove.x * _speed;
            _moveVector.z = desiredMove.z * _speed;
        }

        private void GetHorrizontalLaunchInput()
        {
            // Read input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            _input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (_input.sqrMagnitude > 1)
            {
                _input.Normalize();
            }

            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward*_input.y + transform.right*_input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, _characterController.radius / 2, Vector3.down, out hitInfo,
                               _characterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            desiredMove.x = desiredMove.x * _launchControl * Time.deltaTime;
            desiredMove.z = desiredMove.z * _launchControl * Time.deltaTime;

            if(_characterController.isGrounded)
            {
                _moveVector.x -= Mathf.Sign(_moveVector.x) * _horrizontalLaunchGroundDampening * Time.deltaTime;
                _moveVector.z -= Mathf.Sign(_moveVector.x) * _horrizontalLaunchGroundDampening * Time.deltaTime;
            }
            else
            {
                _moveVector.x -= Mathf.Sign(_moveVector.x) * _horrizontalLaunchAirDampening * Time.deltaTime;
                _moveVector.z -= Mathf.Sign(_moveVector.x) * _horrizontalLaunchAirDampening * Time.deltaTime;
            }

            if(!CompareToZero(_moveVector.x, 1f)) _moveVector.x += desiredMove.x;
            else _moveVector.x = desiredMove.x; 
            if(!CompareToZero(_moveVector.z, 1f)) _moveVector.z += desiredMove.z;
            else _moveVector.z = desiredMove.z;
            
            _speed = Mathf.Sqrt(_moveVector.x * _moveVector.x + _moveVector.z * _moveVector.z);

            if(_speed < _runSpeed && (!CompareToZero(_input.magnitude)))
            {
                _isLaunchingHorrizontally = false;
            }
        }

        private void Fire(bool push, bool pull)
        {
            if(push) EventManager.TriggerEvent(new WeaponFiredEventArgs(WeaponFiredEventArgs.FireType.Push));
            if(pull) EventManager.TriggerEvent(new WeaponFiredEventArgs(WeaponFiredEventArgs.FireType.Pull));
        }

        private IEnumerator SetFOV()
        {
            float timer = 0f;

            float mainFrom = _startingFov;
            float mainTo = _startingFov - _zoomedFovChange;
            float characterFrom = _startingCharacterFOV;
            float characterTo = _startingCharacterFOV - _zoomedFovChange;

            while(timer < _zoomTime && _isAiming)
            {
                _camera.fieldOfView = Mathf.SmoothStep(mainFrom, mainTo, timer / _zoomTime);
                _characterCamera.fieldOfView = Mathf.SmoothStep(characterFrom, characterTo, timer / _zoomTime);
                timer += Time.deltaTime;
                yield return null;
            }

            _zoomTime = timer;
            timer = 0f;

            mainFrom = _camera.fieldOfView;
            mainTo = _startingFov;
            characterFrom = _camera.fieldOfView;
            characterTo = _startingCharacterFOV;

            while(_isAiming)
            {
                yield return null;
            }

            while(timer < _zoomTime)
            {
                _camera.fieldOfView = Mathf.SmoothStep(mainFrom, mainTo, timer / _zoomTime);
                _characterCamera.fieldOfView = Mathf.SmoothStep(characterFrom, characterTo, timer / _zoomTime);
                timer += Time.deltaTime;
                yield return null;
            }

            _camera.fieldOfView = mainTo;
            _characterCamera.fieldOfView = characterTo;

            _isZoomed = false;
        }


        private void RotateView()
        {
            _mouseLook.LookRotation (transform, _camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (_collisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(_characterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
