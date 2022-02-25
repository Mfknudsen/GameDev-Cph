#region Packages

using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

#endregion

namespace GameDev.Multiplayer
{
    public class HostManager : MonoBehaviourPunCallbacks
    {
        #region Values

        public static HostManager instance;

        private PhotonView pv;

        private readonly List<PlayerManager> playerManagers = new List<PlayerManager>();

        //Teams
        private int playerPerTeam;
        private int[] actualPlayerCount = new int[2];

        #endregion

        #region Build In States

        private void Start()
        {
            name = name.Replace("(Clone)", "");

            if (instance != null)
                PhotonNetwork.Destroy(gameObject);

            instance = this;

            pv ??= GetComponent<PhotonView>();

            playerPerTeam = (PhotonNetwork.CurrentRoom.MaxPlayers - 1) / 2;
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }

        #endregion

        #region Setters

        public void AddPlayerManager(PlayerManager add)
        {
            playerManagers.Add(add);
        }

        public void RemovePlayerManager(PlayerManager remove)
        {
            playerManagers.Remove(remove);
        }

        #endregion

        #region In

        public void SpawnComputerControlled(string gameObjectName, Vector3 position, Quaternion rotation)
        {
            pv.RPC("RPCSpawnComputerControlled", RpcTarget.MasterClient, gameObjectName, position, rotation);
        }

        public void SetTeam(string userID, Team team)
        {
            pv.RPC("RPCAssignTeamToPlayer", RpcTarget.MasterClient, userID, team);
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

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCAssignTeamToPlayer(string userID, Team team)
        {
            if (actualPlayerCount[(int) team] == playerPerTeam)
                return;

            actualPlayerCount[(int) team]++;
            
            playerManagers
                .First(pm => pm.GetPhotonView().Owner.UserId.Equals(userID))
                .SetTeam(team);
        }

        #endregion

        #endregion
    }
}