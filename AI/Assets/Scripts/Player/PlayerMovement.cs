using UnityEngine;

namespace Player
{
    /// <summary>
    /// Handles player movement: walking, jumping and drag.
    /// Attach to the Player root GameObject.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Walk")]
        public float moveSpeed = 4f;

        [Header("Jump")]
        public float jumpForce = 5f;

        [Header("Drag")]
        public float groundDrag = 6f;
        public float airDrag = 1f;

        [Header("Ground Check")]
        public float rayLength = 1.1f;
        public LayerMask groundLayer;
        public float groundCheckRadius;

        // ── Internal ──────────────────────────────────────────────────────────────

        private Rigidbody _rb;
        private PlayerInputController _input;
        private PlayerCamera _playerCamera;
        private Vector2 _moveInput;
        private bool _isGrounded;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _input = GetComponent<PlayerInputController>();
            _playerCamera = GetComponent<PlayerCamera>();

            _rb.freezeRotation = true;
        }

        void OnEnable()
        {
            _input.OnMoveEvent += OnMove;
        }

        void OnDisable()
        {
            _input.OnMoveEvent -= OnMove;
        }

        void Update()
        {
            CheckGround();
            //ApplyDrag();
            _playerCamera.SetMoving(_moveInput != Vector2.zero && _isGrounded);
        }

        void FixedUpdate()
        {
            Move();
        }

        // ── Input handlers ────────────────────────────────────────────────────────

        void OnMove(Vector2 input) => _moveInput = input;

        // ── Private methods ───────────────────────────────────────────────────────

        void Move()
        {
            Vector3 direction = transform.right * _moveInput.x
                                + transform.forward * _moveInput.y;

            _rb.AddForce(direction * moveSpeed, ForceMode.VelocityChange);

            // Cap horizontal speed so the player doesn't accelerate forever
            Vector3 flatVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 capped = flatVelocity.normalized * moveSpeed;
                _rb.linearVelocity = new Vector3(capped.x, _rb.linearVelocity.y, capped.z);
            }
        }

        void Jump()
        {
            // Reset vertical velocity for a consistent jump height
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        void CheckGround()
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            Vector3 origin = transform.position + Vector3.up * (groundCheckRadius + 0.1f);
            //_isGrounded = Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out _, rayLength, groundLayer);
            _isGrounded = Physics.Raycast(origin, Vector3.down, rayLength, groundLayer);
            Debug.DrawRay(origin, Vector3.down * rayLength, _isGrounded ? Color.green : Color.red);
        }

        void ApplyDrag()
        {
            _rb.linearDamping = _isGrounded ? groundDrag : airDrag;
        }
    }
}