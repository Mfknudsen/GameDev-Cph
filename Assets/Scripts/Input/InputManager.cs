#region Packages

using UnityEngine;
using UnityEngine.Events;

#endregion

namespace GameDev.Input
{
    public class InputManager
    {
        #region Values

        public static InputManager Instance
        {
            get
            {
                instance ??= new InputManager();

                return instance;
            }
        }

        private static InputManager instance;

        public UnityEvent<Vector2> moveEvent = new UnityEvent<Vector2>(),
            rotEvent = new UnityEvent<Vector2>();

        public UnityEvent interactEvent = new UnityEvent(),
            jumpEvent = new UnityEvent(),
            shootEvent = new UnityEvent(),
            reloadEvent = new UnityEvent(),
            meleeEvent = new UnityEvent(),
            buildEvent = new UnityEvent(),
            dropEvent = new UnityEvent(),
            throwEvent = new UnityEvent(),
            crouchEvent = new UnityEvent(),
            pauseEvent = new UnityEvent();

        public UnityEvent<float> mouseScrollEvent = new UnityEvent<float>(),
            turnEvent = new UnityEvent<float>();

        #endregion

        #region Build In States

        private InputManager()
        {
            PlayerInput playerInput = new PlayerInput();
            playerInput.Enable();
            PlayerInput.PlayerActions player = playerInput.Player;

            #region Axis

            //Vectors should be added both on "performed" and "canceled" to know the player is no longer giving input and return value to Vector2.zero
            player.MoveVector.performed += (context) =>
                moveEvent.Invoke(context.ReadValue<Vector2>());
            player.MoveVector.canceled += (context) =>
                moveEvent.Invoke(context.ReadValue<Vector2>());

            player.RotVector.performed += (context) =>
                rotEvent.Invoke(context.ReadValue<Vector2>());
            player.RotVector.canceled += (context) =>
                rotEvent.Invoke(context.ReadValue<Vector2>());

            player.Scroll.performed += (context) =>
                mouseScrollEvent.Invoke(Mathf.Clamp(context.ReadValue<Vector2>().y, -1, 1));
            player.Scroll.canceled += (context) =>
                mouseScrollEvent.Invoke(Mathf.Clamp(context.ReadValue<Vector2>().y, -1, 1));

            player.Turn.performed += (context) =>
                turnEvent.Invoke(context.ReadValue<float>());
            player.Turn.canceled += (context) =>
                turnEvent.Invoke(context.ReadValue<float>());

            #endregion

            #region Button

            //Buttons should only be added to "performed"
            player.Interact.performed += (context) => interactEvent.Invoke();

            player.Jump.performed += (context) => jumpEvent.Invoke();

            player.Shoot.performed += (context) => shootEvent.Invoke();

            player.Reload.performed += (context) => reloadEvent.Invoke();

            player.Melee.performed += (context) => meleeEvent.Invoke();

            player.Build.performed += (context) => buildEvent.Invoke();

            player.Drop.performed += (context) => dropEvent.Invoke();

            player.Throw.performed += (context) => throwEvent.Invoke();

            player.Crouch.performed += (context) => crouchEvent.Invoke();

            player.Pause.performed += (context) => pauseEvent.Invoke();

            #endregion
        }

        #endregion
    }
}