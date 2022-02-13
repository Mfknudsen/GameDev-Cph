#region Packages

using GameDev.Input;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.FPS
{
    public sealed class FPSCamera : MonoBehaviour
    {
        #region Values

        [SerializeField] private PhotonView pv;
        [SerializeField] private float rotSpeed;
        private float dir;
        private Transform objTransform;

        #endregion

        #region Build In States

        private void Start()
        {
            objTransform = transform;

            pv ??= GetComponent<PhotonView>();

            if (pv.IsMine)
                InputManager.instance.rotEvent.AddListener(OnRotAxisUpdate);
        }

        private void Update()
        {
            if (!pv.IsMine) return;

            Vector3 eulerAngles = objTransform.rotation.eulerAngles;
            eulerAngles.x += dir * rotSpeed * Time.deltaTime;

            float check = eulerAngles.x - 180;
            if (check < 100 && check > 0)
                eulerAngles.x = 280;
            else if (check > -100 && check < 0)
                eulerAngles.x = 80;

            objTransform.rotation = Quaternion.Euler(eulerAngles);
        }

        #endregion

        #region Internal

        private void OnRotAxisUpdate(Vector2 input)
        {
            dir = -input.y;
        }

        #endregion
    }
}