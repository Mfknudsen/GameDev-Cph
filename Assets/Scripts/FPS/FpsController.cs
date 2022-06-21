#region Packages

using GameDev.Character;
using GameDev.Common;
using GameDev.Input;
using GameDev.UI.FPS;
using GameDev.Weapons;
using UnityEngine;

#endregion

namespace GameDev.FPS
{
    public class FpsController : Controller
    {
        #region Values

        [SerializeField] private Weapon weapon;

        [SerializeField] protected float moveSpeed,
            rotSpeed,
            distance,
            jumpForce,
            timeBetweenJump,
            cayotyTime;

        [SerializeField] protected LayerMask groundedMask;

        protected bool isGrounded,
            jumping;

        protected Timer jumpTimer, cayotyTimer;

        protected Rigidbody rb;

        #endregion

        #region Build In States

        protected override void Start()
        {
            base.Start();

            rb ??= GetComponent<Rigidbody>();

            if (!pv.IsMine)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (pv.IsMine)
                InputManager.instance.jumpEvent.AddListener(OnJumpUpdate);
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (pv.IsMine)
                InputManager.instance.jumpEvent.RemoveListener(OnJumpUpdate);
        }

        protected virtual void Update()
        {
            if (!pv.IsMine) return;

            Rotate();
            Jump();
        }

        protected virtual void FixedUpdate()
        {
            if (!pv.IsMine) return;

            GroundDetect();
            Move();
        }

        #endregion

        #region Getters

        public bool GetIsGrounded()
        {
            return isGrounded;
        }

        #endregion

        #region In

        public virtual void SetupUI(FpsUI ui)
        {
            ui.SetWeaponToDisplay(weapon);
            ui.SetHealthToDisplay(GetComponent<Health>());
        }

        #endregion

        #region Internal

        protected virtual void Move()
        {
            Vector3 forward = moveDir.y * objTransform.forward,
                side = moveDir.x * objTransform.right;
            rb.MovePosition(objTransform.position += (forward + side).normalized * (moveSpeed * Time.deltaTime));
        }

        private void Rotate()
        {
            transform.Rotate(transform.up, rotDir.x * rotSpeed * Time.deltaTime);
        }

        protected virtual void Jump()
        {
            if ((!isGrounded && cayotyTimer == null) || !jumping || jumpTimer != null) return;

            rb.velocity = Vector3.zero;
            rb.AddForce(objTransform.up * jumpForce, ForceMode.Impulse);

            isGrounded = false;
            jumpTimer = new Timer(TimerType.Seconds, timeBetweenJump);
            jumpTimer.timerEvent.AddListener(() => jumpTimer = null);
        }

        protected virtual void GroundDetect()
        {
            if (isGrounded || jumpTimer != null) return;
            
            Ray ray = new Ray(objTransform.position, -objTransform.up);
            if (Physics.Raycast(ray, distance, groundedMask))
                isGrounded = true;
            
            Debug.DrawRay(ray.origin, ray.direction, Color.white);
        }

        #region Input

        private void OnJumpUpdate()
        {
            jumping = !jumping;
        }

        #endregion

        #endregion
    }
}