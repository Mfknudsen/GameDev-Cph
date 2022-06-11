#region Packages

using GameDev.Buildings;
using GameDev.Common;
using GameDev.FPS;
using GameDev.Input;
using GameDev.UI.RTS;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace GameDev.RTS
{
    public class RtsController : Controller
    {
        #region Values

        [SerializeField] private GameObject uiPrefab;

        [SerializeField] private int borderWidth;

        [SerializeField] private float moveSpeed, rotTurnSpeed, rotMouseSpeed;

        private float turnDir, mouseDir;

        private bool freeLook;

        private CameraController cameraController;

        private Vector2 borderDir;

        private CommandBuilding commandBuilding;

        private GameObject uiObject;

        #endregion

        #region Build In States

        protected override void Start()
        {
            base.Start();

            if (!pv.IsMine) return;

            cameraController = GetComponentInChildren<CameraController>();
            new Timer(TimerType.Seconds,0.01f).timerEvent.AddListener(() => cameraController.enabled = false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            uiObject = Instantiate(uiPrefab, GameObject.Find("Canvas").transform);
            uiObject.GetComponent<RtsUI>().Setup(this);
        }

        private void Update()
        {
            if (!pv.IsMine) return;

            if (!freeLook)
                BorderDetect();
            else
                borderDir = Vector2.zero;

            Move();
            Rotate();
        }

        public override void OnEnable()
        {
            if (pv.IsMine)
            {
                InputManager.instance.turnEvent.AddListener(OnTurnUpdate);
                InputManager.instance.crouchEvent.AddListener(OnCrouchUpdate);
                InputManager.instance.rotEvent.AddListener(OnMouseUpdate);
            }
        }

        public override void OnDisable()
        {
            if (pv.IsMine)
            {
                InputManager.instance.turnEvent.RemoveListener(OnTurnUpdate);
                InputManager.instance.crouchEvent.RemoveListener(OnCrouchUpdate);
                InputManager.instance.rotEvent.RemoveListener(OnMouseUpdate);
            }
        }

        private void OnDestroy()
        {
            Destroy(uiObject);
        }

        #endregion

        #region Getters

        public CommandBuilding GetCommandBuilding()
        {
            return commandBuilding;
        }

        #endregion

        #region In

        public void Setup(CommandBuilding building)
        {
            commandBuilding = building;
        }

        #endregion

        #region Internal

        private void BorderDetect()
        {
            Vector2 mousePos = Mouse.current.position.ReadUnprocessedValue(),
                screenSize = new Vector2(Screen.width, Screen.height);

            //X
            float x;
            if (mousePos.x < borderWidth)
                x = -1;
            else if (mousePos.x > screenSize.x - borderWidth)
                x = 1;
            else
                x = 0;

            //Y
            float y;
            if (mousePos.y < borderWidth)
                y = -1;
            else if (mousePos.y > screenSize.y - borderWidth)
                y = 1;
            else
                y = 0;

            borderDir = new Vector2(x, y);
        }

        private void Move()
        {
            Vector2 useVector = freeLook ? moveDir : borderDir;

            Vector3 forward = useVector.y * objTransform.forward,
                side = useVector.x * objTransform.right;

            objTransform.position += (forward + side).normalized * (moveSpeed * Time.deltaTime);
        }

        private void Rotate()
        {
            objTransform.Rotate(
                Vector3.up,
                (freeLook ? mouseDir * rotMouseSpeed : turnDir * rotTurnSpeed)
                * Time.deltaTime);
        }

        #region Input

        private void OnMouseUpdate(Vector2 input)
        {
            mouseDir = input.x;
        }

        private void OnTurnUpdate(float input)
        {
            turnDir = Mathf.Clamp(input, -1, 1);
        }

        private void OnCrouchUpdate()
        {
            freeLook = !freeLook;

            Cursor.visible = !freeLook;
            Cursor.lockState = freeLook ? CursorLockMode.Locked : CursorLockMode.Confined;
            cameraController.enabled = freeLook;
        }

        #endregion

        #endregion
    }
}