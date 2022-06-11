#region Packages

using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GameDev.Character;
using GameDev.Input;
using GameDev.Terrain.Doors;
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

            if (!pv.IsMine)
                return;

            Hashtable hash = new Hashtable();
            hash.Add("Stat" + PlayerStat.WeaponLevel.ToString(), 1);
            hash.Add("Stat" + PlayerStat.ArmorLevel.ToString(), 1);

            hash.Add("HumanCount", 0);
            hash.Add("AlienCount", 0);

            pv.Owner.SetCustomProperties(hash);
        }

        public override void OnEnable()
        {
            InputManager.instance.jumpEvent.AddListener(OnJumpChange);
        }

        public override void OnDisable()
        {
            InputManager.instance.jumpEvent.RemoveListener(OnJumpChange);
        }

        #endregion

        #region Getters

        public PhotonView GetPhotonView()
        {
            return pv;
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

        public void SpawnBuilding(string gameObjectName, Vector3 position, Quaternion rotation)
        {
            pv.RPC("RPCSpawnBuilding", RpcTarget.MasterClient, gameObjectName, position, rotation);
        }

        public void SetHostState(string hashKey, object hashValue)
        {
            if (hashKey == null || hashKey.Equals("") || !pv.IsMine) return;

            Hashtable hash = new Hashtable();
            hash.Add(hashKey, hashValue);
            PhotonNetwork.MasterClient.SetCustomProperties(hash);

            pv.RPC("SyncOnHostStateChange", RpcTarget.Others);
        }

        public void EndGame(Team teamWon)
        {
            playerManagers.ForEach(m => m.EndGame(teamWon));
        }

        #endregion

        #region Internal

        private void OnJumpChange()
        {
            if (!pv.IsMine) return;

            if (pv.Owner.CustomProperties.ContainsKey("Start")) return;

            Hashtable hash = new Hashtable();
            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            hash.Add("Start", System.DateTime.Now.ToString());
            pv.Owner.SetCustomProperties(hash);

            foreach (Health health in FindObjectsOfType<BreakableDoor>().Select(b => b.GetComponent<Health>()))
                health.InstantKill();

            FindObjectsOfType<Objective>().First(o => o.GetFirst()).StartObjective();
        }

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
                if (actualPlayerCount[(int)team - 1] == playerPerTeam)
                    return;

                actualPlayerCount[(int)team - 1]++;

                pv.RPC("SyncPlayerCounts", RpcTarget.Others, actualPlayerCount[0], actualPlayerCount[1]);

                playerManagers
                    .First(pm =>
                        pm.GetPhotonView().Owner.NickName.Equals(userNickName))
                    .SetTeam(team);
            }
        }

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCSpawnBuilding(string gameObjectName, Vector3 position, Quaternion rotation)
        {
            PhotonNetwork.Instantiate(gameObjectName, position, rotation);
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

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void SyncOnHostStateChange()
        {
            PlayerManager.ownedManager.OnHostStateChange();
        }

        #endregion

        #endregion

        #endregion
    }
}