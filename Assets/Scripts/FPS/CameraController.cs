#region Packages

using Cinemachine;
using GameDev.Input;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.FPS
{
    public sealed class CameraController : MonoBehaviour
    {
        #region Values

        [SerializeField] private PhotonView pv;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private float rotSpeed;
        [SerializeField] private float minAngle, maxAngle;
        private float dir, current;
        private Transform objTransform;

        #endregion

        #region Build In States

        private void Start()
        {
            current = maxAngle - Mathf.Abs(minAngle);
            objTransform = transform;
            objTransform.localEulerAngles = Vector3.left * current;

            pv ??= GetComponent<PhotonView>();

            if (pv.IsMine)
            {
                InputManager.instance.rotEvent.AddListener(OnRotAxisUpdate);
                virtualCamera.enabled = true;
            }
        }

        private void Update()
        {
            if (!pv.IsMine) return;

            current += dir * rotSpeed * Time.deltaTime;
            current = Mathf.Clamp(current, minAngle, maxAngle);

            objTransform.localEulerAngles = Vector3.left * current;
        }

        #endregion

        #region Getters

        public float GetCurrentAngle()
        {
            return current;
        }

        public float GetMax()
        {
            return maxAngle;
        }

        #endregion

        #region Internal

        private void OnRotAxisUpdate(Vector2 input)
        {
            dir = input.y;
        }

        #endregion
    }
}