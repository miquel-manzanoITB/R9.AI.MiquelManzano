using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

namespace Player
{
    /// <summary>
    /// Single entry point for all player input.
    /// Translates raw input into events that other scripts subscribe to.
    /// Attach to the Player root GameObject.
    /// </summary>
    public class PlayerInputController : MonoBehaviour, IPlayerActions
    {
        // ── Events ────────────────────────────────────────────────────────────────

        public event UnityAction<Vector2> OnMoveEvent = delegate { };
        public event UnityAction<Vector2> OnLookEvent = delegate { };

        // ── Internal ──────────────────────────────────────────────────────────────

        private InputSystem_Actions _inputActions;
        private bool _cameraLocked;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        void Awake()
        {
            _inputActions = new InputSystem_Actions();
            _inputActions.Player.SetCallbacks(this);
        }

        void OnEnable() => _inputActions.Enable();
        void OnDisable() => _inputActions.Disable();

        // ── IPlayerActions callbacks ──────────────────────────────────────────────

        public void OnMove(InputAction.CallbackContext context)
            => OnMoveEvent.Invoke(context.ReadValue<Vector2>());

        public void SetCameraLocked(bool locked) => _cameraLocked = locked;
        public void OnLook(InputAction.CallbackContext context)
        {
            if (!_cameraLocked)
            {
                OnLookEvent.Invoke(context.ReadValue<Vector2>());
            }
            else
            {
                OnLookEvent.Invoke(Vector2.zero);  // ignore look input when camera is locked
            }
        }
    }
}