#region Packages

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

#endregion

namespace GameDev.Input
{
    public class InputManager
    {
        #region Values

        private static InputManager instance;

        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new InputManager();

                return instance;
            }
        }

        private UnityEvent<Vector2> moveEvent,
            rotEvent;

        private UnityEvent interactEvent;

        #endregion

        #region Build In States

        public InputManager()
        {
            #region Setup Events

            PlayerInput.PlayerActions player = new PlayerInput().Player;

            moveEvent = new UnityEvent<Vector2>();
            rotEvent = new UnityEvent<Vector2>();
            interactEvent = new UnityEvent();

            #endregion

            #region Axis

            //Vectors should be added both on "performed" and "canceled" to know the player is no longer giving input
            player.MoveVector.performed += OnMoveAxisPerformed;
            player.MoveVector.canceled += OnMoveAxisPerformed;

            player.RotVector.performed += OnRotAxisPerformed;
            player.RotVector.canceled += OnRotAxisPerformed;

            #endregion

            #region Button

            //Buttons should only be added to "performed"
            player.Interact.performed += OnInteractPerformed;

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

        #endregion

        #region Buttons

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            interactEvent.Invoke();
        }

        #endregion

        #endregion
    }
}