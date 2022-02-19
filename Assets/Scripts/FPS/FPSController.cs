#region Packages

using GameDev.Common;
using GameDev.Input;
using UnityEngine;

#endregion

namespace GameDev.FPS
{
    public class FpsController : Controller
    {
        #region Values

        [SerializeField] protected float moveSpeed,
            rotSpeed,
            distance,
            jumpForce,
            timeBetweenJump;

        [SerializeField] protected LayerMask groundedMask;

        protected bool isGrounded,
            jumping;

        protected Timer jumpTimer;

        protected Rigidbody rb;

        #endregion

        #region Build In States

        protected override void Start()
        {
            base.Start();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            rb = GetComponent<Rigidbody>();

            if (pv.IsMine)
                InputManager.instance.jumpEvent.AddListener(OnJumpUpdate);
            else
                rb.detectCollisions = false;
        }

        protected virtual void Update()
        {
            if (pv.IsMine)
            {
                Rotate();
                Jump();
            }
        }

        protected virtual void FixedUpdate()
        {
            if (pv.IsMine)
            {
                GroundDetect();
                Move();
            }
        }

        #endregion

        #region Internal

        protected virtual void Move()
        {
            Vector3 forward = moveDir.y * objTransform.forward,
                side = moveDir.x * objTransform.right;
            rb.MovePosition(objTransform.position += (forward + side).normalized * (moveSpeed * Time.deltaTime));
        }

        protected virtual void Rotate()
        {
            transform.Rotate(transform.up, rotDir.x * rotSpeed * Time.deltaTime);
        }

        protected virtual void Jump()
        {
            if (!isGrounded || !jumping || jumpTimer != null) return;

            rb.velocity = Vector3.zero;
            rb.AddForce(objTransform.up * jumpForce, ForceMode.Impulse);

            isGrounded = false;
            jumpTimer = new Timer(timeBetweenJump);
            jumpTimer.timerEvent.AddListener(() => jumpTimer = null);
        }

        protected virtual void GroundDetect()
        {
            if (isGrounded || jumpTimer != null) return;

            Ray ray = new Ray(objTransform.position, -objTransform.up);
            if (Physics.Raycast(ray, distance, groundedMask))
                isGrounded = true;
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