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

            if (pv.IsMine)
            {
                InputManager.instance.moveEvent.AddListener(OnMoveAxisUpdate);
                InputManager.instance.rotEvent.AddListener(OnRotAxisUpdate);
            }
            else
            {
                GetComponentInChildren<Camera>().enabled = false;
                GetComponentInChildren<AudioListener>().enabled = false;
            }
        }

        #endregion

        #region In

        public void SetGameObjectActivePun(bool set)
        {
            pv.RPC("GameObjectActive", RpcTarget.All, new object[]{set, pv.Owner.NickName});
        }

        #endregion

        #region Internal

        private void OnMoveAxisUpdate(Vector2 input)
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
        protected void GameObjectActive(bool set, string ownerName)
        {
            if(!pv.Owner.NickName.Equals(ownerName)) return;
                
            gameObject.SetActive(set);
        }

        #endregion

        #endregion
    }
}