#region Packages

using System.Collections.Generic;
using System.Linq;
using GameDev.Buildings;
using GameDev.Common;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Multiplayer
{
    #region Enums

    public enum Team
    {
        Human,
        Alien
    }

    #endregion

    public class PlayerManager : MonoBehaviour
    {
        #region Values

        public static PlayerManager ownedManager;

        private PhotonView pv;

        private Team team = Team.Human;
        private GameObject currentPlayerCharacter;

        public List<SpawnBuilding> spawnPoints = new List<SpawnBuilding>();

        #endregion

        #region Build In States

        private void Start()
        {
            name = name.Replace("(Clone)", "");

            pv ??= GetComponent<PhotonView>();

            if (!pv.IsMine) return;

            ownedManager = this;

            new Timer(0.1f).timerEvent.AddListener(() =>
            {
                spawnPoints.AddRange(FindObjectsOfType<SpawnBuilding>().Where(p => !spawnPoints.Contains(p)));
                TrySpawn();
            });
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

        #endregion

        #region In

        public GameObject CreateController(GameObject controllerPrefab, Vector3 spawnPosition, Quaternion spawnRotation)
        {
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
            PhotonNetwork.Destroy(currentPlayerCharacter);
            currentPlayerCharacter = null;

            Timer respawnTimer = new Timer(1);
            respawnTimer.timerEvent.AddListener(() =>
                spawnPoints[Random.Range(0, spawnPoints.Count - 1)].SpawnController(this));
        }

        #endregion

        #region Internal

        private void TrySpawn()
        {
            if (spawnPoints.Count > 0)
            {
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

        #endregion
    }
}