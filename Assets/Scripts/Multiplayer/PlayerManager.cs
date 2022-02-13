#region Packages

using System.Collections.Generic;
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

        private PhotonView pv;

        private Team team = Team.Human;
        private GameObject currentPlayerCharacter;

        private List<SpawnPlayer> spawnPoints = new List<SpawnPlayer>();

        #endregion

        #region Build In States

        private void Start()
        {
            spawnPoints.AddRange(FindObjectsOfType<SpawnPlayer>());

            Cursor.lockState = CursorLockMode.Confined;

            pv ??= GetComponent<PhotonView>();

            if (!pv.IsMine) return;

            SpawnPlayer s = spawnPoints[Random.Range(0, spawnPoints.Count - 1)];
            s.Spawn(this);
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

        public void CreateController(GameObject controllerPrefab, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            currentPlayerCharacter =
                PhotonNetwork.Instantiate(controllerPrefab.name, spawnPosition, spawnRotation);
        }

        public void Die()
        {
            PhotonNetwork.Destroy(currentPlayerCharacter);
            currentPlayerCharacter = null;

            Timer respawnTimer = new Timer(1);
            respawnTimer.timerEvent.AddListener(() =>
                spawnPoints[Random.Range(0, spawnPoints.Count - 1)].Spawn(this));
        }

        #endregion
    }
}