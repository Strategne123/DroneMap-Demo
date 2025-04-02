using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Invector.vCharacterController
{
    public class vThirdPersonInput : MonoBehaviour
    {
        #region Variables       
        public bool InputIsOn;
        [Header("Controller Input")]
        public string horizontalInput = "Horizontal";
        public string verticalInput = "Vertical";
        public string joystickYaw = "j4";
        public string joystickThrust = "j3";
        public string joystickRoll = "Horizontal";
        public string joystickPitch = "Vertical";
        public KeyCode jumpInput = KeyCode.Space;
        public KeyCode strafeInput = KeyCode.Tab;
        public KeyCode sprintInput = KeyCode.LeftShift;

        public float cameraSensitivity = 1.0f;
        public float cameraDeadZone = 0.1f;

        [Header("Camera Input")]
        public string rotateCameraXInput = "j1"; // Joystick input for X axis
        public string rotateCameraYInput = "j2"; // Joystick input for Y axis
        public Camera customCamera { get; set; } // Пользовательская камера, которую можно выбрать в инспекторе
        public float cameraHeight = 1.5f; // Настраиваемая высота камеры
        public Vector3 cameraOffset = new Vector3(0, 1.5f, -3); // Смещение камеры относительно персонажа

        [HideInInspector] public vThirdPersonController cc;

        private float yaw;
        private float thrust;
        private float roll;
        private float pitch;

        private bool readyToJump;

        #endregion

        protected virtual void Awake()
        {
            InitilizeController();
            InitializeCustomCamera();
            // Сразу включаем strafe режим при запуске
            cc.isStrafing = true;
        }

        protected virtual void FixedUpdate()
        {
            cc.UpdateMotor();               // updates the ThirdPersonMotor methods
            cc.ControlLocomotionType();     // handle the controller locomotion type and movespeed
            cc.ControlRotationType();       // handle the controller rotation type
        }

        protected virtual void Update()
        {
            InputHandle();                  // update the input methods
            cc.UpdateAnimator();            // updates the Animator Parameters
        }

        protected virtual void LateUpdate()
        {
            //FollowCharacter();
        }

        public virtual void OnAnimatorMove()
        {
            cc.ControlAnimatorRootMotion(); // handle root motion animations 
        }

        #region Basic Locomotion Inputs

        protected virtual void InitilizeController()
        {
            cc = GetComponent<vThirdPersonController>();

            if (cc != null)
                cc.Init();
        }

        protected virtual void InitializeCustomCamera()
        {
            //if (customCamera == null)
            //{
            //    Debug.LogWarning("Custom camera is not assigned. Trying to find the Main Camera.");
            //    customCamera = Camera.main;
            //    if (customCamera == null)
            //    {
            //        Debug.LogError("Main Camera is not found. Please assign a camera.");
            //        return;
            //    }
            //}
            cc.rotateTarget = transform;
        }

        protected virtual void InputHandle()
        {
            UpdateController();  // Get joystick inputs
            MoveInput();
            CameraInput();
            SprintInput();
            StrafeInput();
            JumpInput();

            //if (customCamera.transform.localRotation.y != 0)
            //{
            //    transform.Rotate(Vector3.up, customCamera.transform.rotation.eulerAngles.y, Space.World);
            //    customCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
            //}

        }

        protected virtual void UpdateController()
        {
            if (InputIsOn)
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                yaw = Input.GetAxis("j4");
                thrust = Input.GetAxis("j3");
                roll = Input.GetAxis("Horizontal"); // Inverted horizontal input
                pitch = -Input.GetAxis("Vertical");
#else
            yaw = Input.GetAxis("j3");
            thrust = Input.GetAxis("j14");
            roll = Input.GetAxis("Horizontal"); // Inverted horizontal input
            pitch = -Input.GetAxis("Vertical");
#endif
            }
            else
            {
                yaw = 0;
                thrust = 0;
                roll = 0;
                pitch = 0;
            }

        }

        public virtual void MoveInput()
        {
            cc.input.x = roll;
            cc.input.z = pitch;
        }

        protected virtual void CameraInput()
        {
            if (customCamera)
            {
                cc.UpdateMoveDirection(transform);

                var X = yaw;

                // Apply dead zone
                if (Mathf.Abs(X) < cameraDeadZone) X = 0;

                // Apply sensitivity
                X *= cameraSensitivity;

                transform.Rotate(Vector3.up, X, Space.World);
            }
        }

        protected virtual void FollowCharacter()
        {
            if (customCamera)
            {
                Vector3 desiredPosition = transform.position + cameraOffset;
                customCamera.transform.position = desiredPosition;
                customCamera.transform.LookAt(transform.position + Vector3.up * cameraHeight);
            }
        }

        protected virtual void StrafeInput()
        {
            // Убираем переключение режима, оставляем только strafe режим
            if (!cc.isStrafing)
            {
                cc.Strafe();
            }
        }

        protected virtual void SprintInput()
        {
            if (Input.GetKeyDown(sprintInput))
                cc.Sprint(true);
            else if (Input.GetKeyUp(sprintInput))
                cc.Sprint(false);
        }

        protected virtual bool JumpConditions()
        {
            return cc.isGrounded && cc.GroundAngle() < cc.slopeLimit && !cc.isJumping && !cc.stopMove;
        }

        protected virtual void JumpInput()
        {
            if (thrust >= 0.8f)
            {

                if (readyToJump && JumpConditions())
                {
                    readyToJump = false;
                    cc.Jump();
                }
            }
            else if (thrust <= -0.8f)
            {
                //Crouch();
            }
            else
            {
                if (JumpConditions())
                {
                    readyToJump = true;
                }
            }

        }

        protected virtual void Crouch()
        {
            // Implement crouch logic here
        }

        #endregion       
    }
}
