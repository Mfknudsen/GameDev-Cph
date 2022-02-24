#region Packages

using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Multiplayer
{
    public class HostManager : MonoBehaviourPunCallbacks
    {
        #region Values

        public static HostManager instance;

        private PhotonView pv;

        #endregion

        #region Build In States

        private void Start()
        {
            name = name.Replace("(Clone)", "");

            if (instance != null)
                PhotonNetwork.Destroy(gameObject);

            instance = this;
        }

        #endregion

        #region In

        public void SpawnComputerControlled(string gameObjectName, Vector3 position, Quaternion rotation)
        {
            pv.RPC("RPCSpawnComputerControlled", RpcTarget.MasterClient, gameObjectName, position, rotation);
        }

        #endregion

        #region Internal

        #region Pun RPC

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCSpawnComputerControlled(string gameObjectName, Vector3 position, Quaternion rotation)
        {
            PhotonNetwork.Instantiate(gameObjectName, position, rotation);
        }

        #endregion

        #endregion
    }
}