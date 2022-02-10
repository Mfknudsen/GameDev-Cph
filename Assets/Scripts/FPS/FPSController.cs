#region Packages

using GameDev.Input;
using UnityEngine;

#endregion

namespace GameDev.FPS
{
    public class FPSController : Controller
    {
        #region Values

        [SerializeField] private float moveSpeed,
            rotSpeed,
            distance,
            jumpForce;

        [SerializeField] private LayerMask layerMask;
        
        protected bool jumping, isGrounded;

        private Timer jumpTimer;

        private Rigidbody rb;

        #endregion

        #region Internal

        protected override void StartOwned()
        {
            base.StartOwned();

            rb = GetComponent<Rigidbody>();

            InputManager.instance.jumpEvent.AddListener(OnJumpUpdate);
        }

        protected override void StartUnowned()
        {
            base.StartUnowned();

            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
        }

        protected override void UpdateOwned()
        {
            jumpTimer?.Update();

            Rotate();
            Move();
            Jump();
            GroundDetect();
        }

        protected virtual void Move()
        {
            Vector3 forward = moveDir.y * objTransform.forward,
                side = moveDir.x * objTransform.right;
            objTransform.position += (forward + side).normalized * (moveSpeed * Time.deltaTime);
        }

        protected virtual void Rotate()
        {
            transform.Rotate(transform.up, rotDir.x * rotSpeed * Time.deltaTime);
        }

        protected virtual void Jump()
        {
            if (!isGrounded) return;

            if (!jumping) return;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            isGrounded = false;
            jumpTimer = new Timer(0.05f);
        }

        protected virtual void GroundDetect()
        {
            if (isGrounded) return;

            if (jumpTimer != null && !jumpTimer.IsDone()) return;

            Ray ray = new Ray(objTransform.position, -objTransform.up);
            if (Physics.Raycast(ray, distance, layerMask))
                isGrounded = true;
            Debug.DrawRay(ray.origin, ray.direction * distance);
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