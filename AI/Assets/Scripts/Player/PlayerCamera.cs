using UnityEngine;

namespace Player
{
    /// <summary>
    /// Controls the first-person camera: look, FOV and optional head bob.
    /// Attach to the Player root GameObject alongside PlayerMovement.
    /// </summary>
    public class PlayerCamera : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Camera child transform — used for vertical pitch and bob.")]
        public Transform cameraTransform;

        [Header("Sensitivity")]
        public float sensitivityX = 0.2f;
        public float sensitivityY = 0.2f;

        [Header("Pitch Clamp")]
        public float minPitch = -80f;
        public float maxPitch = 80f;

        [Header("Field of View")]
        public float defaultFOV = 75f;
        [Tooltip("FOV applied while the player is walking.")]
        public float walkFOV = 78f;
        [Tooltip("Speed of FOV transitions.")]
        public float fovLerpSpeed = 6f;

        [Header("Camera Bob")]
        public bool bobEnabled = true;
        [Tooltip("How fast the camera bobs while walking.")]
        public float bobFrequency = 8f;
        [Tooltip("How much the camera moves up/down while walking.")]
        public float bobAmplitude = 0.04f;

        // ── Internal ──────────────────────────────────────────────────────────────

        private Camera _cam;
        private PlayerInputController _input;
        private Vector2 _lookInput;
        private float _cameraPitch;
        private float _bobTimer;
        private Vector3 _bobOffset;
        private bool _isMoving;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        void Awake()
        {
            _input = GetComponent<PlayerInputController>();
            _cam = cameraTransform.GetComponent<Camera>();
            _cam.fieldOfView = defaultFOV;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void OnEnable() => _input.OnLookEvent += OnLook;
        void OnDisable() => _input.OnLookEvent -= OnLook;

        void Update()
        {
            HandleLook();
            HandleFOV();

            if (bobEnabled) HandleBob();
        }

        // ── Input handler ─────────────────────────────────────────────────────────

        void OnLook(Vector2 input) => _lookInput = input;

        // Called by PlayerMovement each frame
        public void SetMoving(bool moving) => _isMoving = moving;

        // ── Private methods ───────────────────────────────────────────────────────

        void HandleLook()
        {
            // Horizontal → rotate the Player root
            transform.Rotate(Vector3.up * _lookInput.x * sensitivityX);

            // Vertical → tilt only the camera child
            _cameraPitch -= _lookInput.y * sensitivityY;
            _cameraPitch = Mathf.Clamp(_cameraPitch, minPitch, maxPitch);
            cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
        }

        void HandleFOV()
        {
            float targetFOV = _isMoving ? walkFOV : defaultFOV;
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);
        }

        void HandleBob()
        {
            if (_isMoving)
            {
                _bobTimer += Time.deltaTime * bobFrequency;
                _bobOffset = new Vector3(0f, Mathf.Sin(_bobTimer) * bobAmplitude, 0f);
            }
            else
            {
                _bobTimer = 0f;
                _bobOffset = Vector3.Lerp(_bobOffset, Vector3.zero, Time.deltaTime * fovLerpSpeed);
            }

            cameraTransform.localPosition = _bobOffset;
        }

        // ── Public helpers (Settings menu) ────────────────────────────────────────

        public void SetSensitivity(float x, float y) { sensitivityX = x; sensitivityY = y; }
        public void SetFOV(float fov) { defaultFOV = fov; walkFOV = fov + 3f; }
        public void SetBobEnabled(bool enabled) => bobEnabled = enabled;
    }
}