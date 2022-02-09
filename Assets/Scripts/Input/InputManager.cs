#region Packages

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

#endregion

namespace GameDev.Input
{
    public class InputManager : MonoBehaviour
    {
        #region Values

        public static InputManager instance;

        [SerializeField] public UnityEvent<Vector2> moveEvent,
            rotEvent;

        [SerializeField] public UnityEvent interactEvent,
            jumpEvent;

        [SerializeField] public UnityEvent<float> mouseScrollEvent;

        #endregion

        #region Build In States

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            #region Setup Events

            PlayerInput playerInput = new PlayerInput();
            playerInput.Enable();
            PlayerInput.PlayerActions player = playerInput.Player;

            moveEvent = new UnityEvent<Vector2>();
            rotEvent = new UnityEvent<Vector2>();
            interactEvent = new UnityEvent();
            jumpEvent = new UnityEvent();
            mouseScrollEvent = new UnityEvent<float>();

            #endregion

            #region Axis

            //Vectors should be added both on "performed" and "canceled" to know the player is no longer giving input and return value to Vector2.zero
            player.MoveVector.performed += OnMoveAxisPerformed;
            player.MoveVector.canceled += OnMoveAxisPerformed;

            player.RotVector.performed += OnRotAxisPerformed;
            player.RotVector.canceled += OnRotAxisPerformed;

            player.MouseScroll.performed += OnMouseScrollPerformed;
            player.MouseScroll.canceled += OnMouseScrollPerformed;

            #endregion

            #region Button

            //Buttons should only be added to "performed"
            player.Interact.performed += OnInteractPerformed;

            player.Jump.performed += OnJumpPerformed;

            #endregion
        }

        #endregion

        #region Internal

        #region Axis

        private void OnMoveAxisPerformed(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            moveEvent.Invoke(input);
        }

        private void OnRotAxisPerformed(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            rotEvent.Invoke(input);
        }

        private void OnMouseScrollPerformed(InputAction.CallbackContext context)
        {
            float input = context.ReadValue<float>();
            mouseScrollEvent.Invoke(input);
        }

        #endregion

        #region Buttons

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            interactEvent.Invoke();
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            jumpEvent.Invoke();
        }

        #endregion

        #endregion
    }
}