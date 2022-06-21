#region Packages

using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Input
{
    public abstract class Controller : MonoBehaviourPunCallbacks
    {
        #region Values

        [SerializeField] protected PhotonView pv;
        protected Transform objTransform;
        protected Vector2 moveDir, rotDir;

        #endregion

        #region Build In States

        protected virtual void Start()
        {
            objTransform = transform;

            pv ??= GetComponent<PhotonView>();
        }

        public override void OnEnable()
        {
            if (pv.IsMine)
            {
                InputManager.instance.moveEvent.AddListener(OnMoveAxisUpdate);
                InputManager.instance.rotEvent.AddListener(OnRotAxisUpdate);
            } 
        }

        public override void OnDisable()
        {
            if (pv.IsMine)
            {
                InputManager.instance.moveEvent.RemoveListener(OnMoveAxisUpdate);
                InputManager.instance.rotEvent.RemoveListener(OnRotAxisUpdate);
            } 
        }

        #endregion

        #region In

        public void SetGameObjectActivePun(bool set)
        {
            pv.RPC("GameObjectActive", RpcTarget.All, new object[]{set, pv.Owner.UserId});
        }

        #endregion

        #region Internal

        protected virtual void OnMoveAxisUpdate(Vector2 input)
        {
            moveDir = input;
        }

        private void OnRotAxisUpdate(Vector2 input)
        {
            rotDir = input;
        }

        #region Pun RPC

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        protected void GameObjectActive(bool set, string id)
        {
            if(!pv.Owner.UserId.Equals(id)) return;
                
            gameObject.SetActive(set);
        }

        #endregion

        #endregion
    }
}