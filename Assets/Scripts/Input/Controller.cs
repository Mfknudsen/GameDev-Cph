#region Packages

using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Input
{
    public abstract class Controller : MonoBehaviour
    {
        #region Values

        [SerializeField] private PhotonView photonView;
        protected Transform objTransform;
        protected Vector2 moveDir, rotDir;

        #endregion

        #region Build In States

        protected virtual void Start()
        {
            objTransform = transform;

            photonView ??= GetComponent<PhotonView>();

            if (photonView.IsMine)
                StartOwned();
            else
                StartUnowned();
        }

        private void Update()
        {
            if (photonView.IsMine)
                UpdateOwned();
            else
                UpdateUnowned();
        }

        #endregion

        #region Internal

        protected virtual void StartOwned()
        {
            InputManager.instance.moveEvent.AddListener(OnMoveAxisUpdate);
            InputManager.instance.rotEvent.AddListener(OnRotAxisUpdate);
        }

        protected virtual void StartUnowned()
        {
        }

        protected virtual void UpdateOwned()
        {
        }

        protected virtual void UpdateUnowned()
        {
        }

        private void OnMoveAxisUpdate(Vector2 input)
        {
            moveDir = input;
        }

        private void OnRotAxisUpdate(Vector2 input)
        {
            rotDir = input;
        }

        #endregion
    }
}