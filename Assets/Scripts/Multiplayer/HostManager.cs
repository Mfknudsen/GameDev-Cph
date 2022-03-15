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

        [SerializeField] private GameObject playerManager;

        public static HostManager instance;

        private PhotonView pv;

        private readonly List<PlayerManager> playerManagers = new List<PlayerManager>();

        //Teams
        private int playerPerTeam;
        private readonly int[] actualPlayerCount = new int[2];

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

#if UNITY_EDITOR
            PhotonNetwork.Instantiate(playerManager.name, Vector3.zero, Quaternion.identity);
#endif
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

        #region Owned

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCSpawnComputerControlled(string gameObjectName, Vector3 position, Quaternion rotation)
        {
            PhotonNetwork.Instantiate(gameObjectName, position, rotation);
        }

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCAssignTeamToPlayer(string userNickName, Team team)
        {
            if (team != Team.None)
            {
                if (actualPlayerCount[(int) team - 1] == playerPerTeam)
                    return;

                actualPlayerCount[(int) team - 1]++;

                pv.RPC("SyncPlayerCounts", RpcTarget.Others, actualPlayerCount[0], actualPlayerCount[1]);

                playerManagers
                    .First(pm => pm.GetPhotonView().Owner.NickName.Equals(userNickName))
                    .SetTeam(team);
            }
        }

        #endregion

        #region Sync

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void SyncPlayerCounts(int human, int alien)
        {
            actualPlayerCount[0] = human;
            actualPlayerCount[1] = alien;
        }

        #endregion

        #endregion

        #endregion
    }
}