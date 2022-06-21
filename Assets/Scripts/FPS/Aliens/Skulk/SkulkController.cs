#region Packages

using GameDev.Common;
using UnityEngine;

#endregion

namespace GameDev.FPS.Aliens.Skulk
{
    public sealed class SkulkController : FpsController
    {
        #region Values

        [Space] [Header("Skulk")] [SerializeField]
        private LayerMask climbableMask;

        [SerializeField] private Transform camTransform, moveTransform;

        [SerializeField] private float maxDashSpeed, checkDistance, gravityForce = 9.81f;

        [SerializeField] private Animator anim;

        private Vector3 currentUp = Vector3.up;

        private bool onWall;

        private CameraController cameraController;

        private static readonly int Walking = Animator.StringToHash("Walking");

        #endregion

        #region Build In States

        protected override void Start()
        {
            base.Start();

            if (pv.IsMine)
                cameraController ??= camTransform.gameObject.GetComponentInChildren<CameraController>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!pv.IsMine) return;

            DetectClimbableSurface();
            AddGravity();
        }

        #endregion

        #region Internal

        protected override void Move()
        {
            Transform toUse = onWall
                ? camTransform
                : objTransform;

            Vector3 forward = moveDir.y * toUse.forward,
                side = moveDir.x * toUse.right;

            rb.MovePosition(objTransform.position += (forward + side).normalized * (moveSpeed * Time.deltaTime));
        }

        protected override void Jump()
        {
            Vector3 v = rb.velocity;
            if (v.magnitude > maxDashSpeed)
            {
                Vector3 dash = v;
                dash.y = 0;
                dash = dash.normalized * maxDashSpeed;
                dash.y = v.y;
                rb.velocity = dash;
            }

            if (!jumping && isGrounded && (rb.velocity.x > 0 || rb.velocity.z > 0))
            {
                v.x = 0;
                v.z = 0;
                rb.velocity = v;
            }

            if (!onWall)
                base.Jump();
            else
            {
                if (!isGrounded || !jumping || jumpTimer != null) return;

                float upForce = 0;

                if (cameraController.GetCurrentAngle() > 0)
                {
                    upForce = cameraController.GetCurrentAngle() <= 45
                        ? 1
                        : Mathf.Clamp(
                            (100 - (cameraController.GetCurrentAngle() - 45)
                                / ((cameraController.GetMax() - 45) / 100)) / 100,
                            0,
                            1);
                    upForce *= jumpForce / 2;
                }

                rb.velocity = Vector3.zero;
                rb.AddForce(camTransform.forward * (jumpForce * 2) + camTransform.up * upForce, ForceMode.Impulse);

                isGrounded = false;
                jumpTimer = new Timer(TimerType.Seconds, timeBetweenJump);
                jumpTimer.timerEvent.AddListener(() => jumpTimer = null);
            }
        }

        private void DetectClimbableSurface()
        {
            // ReSharper disable once Unity.PreferNonAllocApi
            Collider[] colliders = Physics.OverlapSphere(
                objTransform.position,
                checkDistance,
                climbableMask,
                QueryTriggerInteraction.Ignore);

            Vector3 closePoint = Vector3.up;

            if (colliders.Length > 0)
            {
                Collider closest = null;
                Vector3 currentPosition = objTransform.position;
                float dist = 0;

                foreach (Collider c in colliders)
                {
                    //Constant warning from this function.
                    //Dont seem to be a problem but cant figure out how to disable.
                    Vector3 pos = c.ClosestPoint(currentPosition);

                    float newDist = Vector3.Distance(pos, currentPosition);

                    if (newDist == 0) continue;

                    if (closest == null)
                    {
                        closest = c;
                        dist = Vector3.Distance(pos, currentPosition);
                        closePoint = pos;
                        continue;
                    }

                    if (newDist < dist)
                    {
                        closePoint = pos;
                        closest = c;
                        dist = newDist;
                    }
                }

                currentUp = closest != null && closePoint != Vector3.zero
                    ? currentUp = (currentPosition - closePoint).normalized
                    : currentUp = Vector3.up;
            }
            else
                currentUp = Vector3.up;

            onWall = currentUp != Vector3.up;

            Quaternion oldRot = moveTransform.rotation;
            moveTransform.rotation = camTransform.rotation;
            moveTransform.LookAt(moveTransform.position + moveTransform.forward, currentUp);
            // ReSharper disable once Unity.InefficientPropertyAccess
            moveTransform.rotation = Quaternion.Lerp(oldRot,
                Quaternion.FromToRotation(moveTransform.up, currentUp) * moveTransform.rotation, 0.9f);
        }

        protected override void GroundDetect()
        {
            Debug.DrawRay(objTransform.position, -currentUp * distance, Color.white);
            if (isGrounded || jumpTimer != null) return;

            Ray ray = new Ray(objTransform.position, -currentUp * distance);
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, distance, groundedMask,
                    QueryTriggerInteraction.Ignore))
            {
                Debug.LogError(hit.collider.gameObject.name);
                isGrounded = true;
            }
        }

        private void AddGravity()
        {
            rb.AddForce(-currentUp * (rb.mass * gravityForce), ForceMode.Force);
        }

        #region Overrides

        protected override void OnMoveAxisUpdate(Vector2 input)
        {
            base.OnMoveAxisUpdate(input);

            if (pv.IsMine)
                anim.SetBool(Walking, input != Vector2.zero);
        }

        #endregion

        #endregion
    }
}