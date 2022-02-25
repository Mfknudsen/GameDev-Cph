#region Packages

using System.Collections.Generic;
using System.Linq;
using GameDev.Buildings;
using GameDev.Common;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

namespace GameDev.Multiplayer
{
    #region Enums

    public enum Team
    {
        None,
        Human,
        Alien
    }

    #endregion

    public class PlayerManager : MonoBehaviour
    {
        #region Values

        public static PlayerManager ownedManager;

        [SerializeField] private GameObject teamSelectUI;

        private PhotonView pv;

        private Team team = Team.None;
        private GameObject currentPlayerCharacter;

        public List<SpawnBuilding> spawnPoints = new List<SpawnBuilding>();

        #endregion

        #region Build In States

        private void Start()
        {
            name = name.Replace("(Clone)", "");

            pv ??= GetComponent<PhotonView>();

            if (PhotonNetwork.IsMasterClient)
                HostManager.instance.AddPlayerManager(this);

            if (!pv.IsMine) return;

            Instantiate(teamSelectUI, GameObject.Find("Canvas").transform);

            ownedManager = this;

            //TrySpawn();
        }

        private void OnDestroy()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            HostManager.instance.RemovePlayerManager(this);
        }

        #endregion

        #region Getters

        public Team GetTeam()
        {
            return team;
        }

        public GameObject GetCurrentPlayerCharacter()
        {
            return currentPlayerCharacter;
        }

        public PhotonView GetPhotonView()
        {
            return pv;
        }

        #endregion

        #region In

        public GameObject CreateController(GameObject controllerPrefab, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (currentPlayerCharacter != null)
                PhotonNetwork.Destroy(currentPlayerCharacter);

            currentPlayerCharacter =
                PhotonNetwork.Instantiate(controllerPrefab.name, spawnPosition, spawnRotation);
            return currentPlayerCharacter;
        }

        public void SwitchCurrentController(GameObject newController)
        {
            currentPlayerCharacter = newController;
        }

        public void Die()
        {
            if (currentPlayerCharacter != null)
            {
                PhotonNetwork.Destroy(currentPlayerCharacter);
                currentPlayerCharacter = null;
            }

            Timer respawnTimer = new Timer(1);
            respawnTimer.timerEvent.AddListener(() =>
            {
                spawnPoints.AddRange(FindObjectsOfType<SpawnBuilding>()
                    .Where(p => !spawnPoints.Contains(p) && p.GetTeam().Equals(team)));
                spawnPoints = spawnPoints
                    .Where(s => s.GetTeam().Equals(team))
                    .ToList();

                spawnPoints[Random.Range(0, spawnPoints.Count - 1)].SpawnController(this);
            });
        }

        public void SetTeam(Team set)
        {
            pv.RPC("RPCSetTeam", RpcTarget.All, set);
        }

        #endregion

        #region Internal

        private void TrySpawn()
        {
            if (spawnPoints.Count > 0 && team != Team.None)
            {
                spawnPoints = spawnPoints.Where(s => s.GetTeam().Equals(team)).ToList();
                SpawnBuilding s = spawnPoints[Random.Range(0, spawnPoints.Count - 1)];
                s.SpawnController(this);
            }
            else
            {
                new Timer(0.01f).timerEvent.AddListener(() =>
                {
                    spawnPoints.AddRange(FindObjectsOfType<SpawnBuilding>().Where(p => !spawnPoints.Contains(p)));
                    TrySpawn();
                });
            }
        }

        #region Pun RPC

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCSetTeam(Team set)
        {
            bool reset = team != set;

            team = set;

            if (!reset || !pv.IsMine) return;

            Die();
        }

        #endregion

        #endregion
    }
}