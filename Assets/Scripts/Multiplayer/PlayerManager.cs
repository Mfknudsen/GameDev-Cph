#region Packages

using System.Collections.Generic;
using System.Linq;
using GameDev.Buildings;
using GameDev.Character;
using GameDev.Common;
using GameDev.Input;
using GameDev.UI.FPS;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

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
        public static List<PlayerManager> allPlayerManagers = new List<PlayerManager>();

        public UnityEvent onPlayerDeath = new UnityEvent();

        [SerializeField] private GameObject teamSelectUI;
        [SerializeField] private PlayerStats stats;

        private PhotonView pv;

        private Team team = Team.None;
        private GameObject currentPlayerCharacter;

        public List<SpawnBuilding> spawnPoints = new List<SpawnBuilding>();

        #endregion

        #region Build In States

        private void OnEnable()
        {
            allPlayerManagers.Add(this);

            InputManager.instance.pauseEvent.AddListener(OnPauseUpdate);
        }

        private void OnDisable()
        {
            allPlayerManagers.Remove(this);

            InputManager.instance.pauseEvent.RemoveListener(OnPauseUpdate);
        }

        private void Start()
        {
            name = name.Replace("(Clone)", "");

            pv ??= GetComponent<PhotonView>();

            if (PhotonNetwork.IsMasterClient)
                HostManager.instance.AddPlayerManager(this);

            if (!pv.IsMine) return;

            if (ownedManager != null)
                Destroy(gameObject);

            ownedManager = this;

            Instantiate(teamSelectUI, GameObject.Find("Canvas").transform);
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

        public PlayerStats GetPlayerStats()
        {
            return stats;
        }

        #endregion

        #region In

        public static GameObject CreateController(GameObject controllerPrefab, Vector3 spawnPosition,
            Quaternion spawnRotation)
        {
            return PhotonNetwork.Instantiate(controllerPrefab.name, spawnPosition, spawnRotation);
        }

        public static GameObject CreateController(GameObject controllerPrefab, Transform spawnTransform)
        {
            return PhotonNetwork.Instantiate(controllerPrefab.name, spawnTransform.position, Quaternion.identity);
        }

        public void SwitchController(GameObject newController)
        {
            if (currentPlayerCharacter != null)
            {
                if (currentPlayerCharacter.GetComponent<VisualManager>() is { } vBefore)
                    vBefore.SetAsNonPlayer();
            }

            currentPlayerCharacter = newController;

            if (currentPlayerCharacter == null) return;

            if (currentPlayerCharacter.GetComponent<VisualManager>() is { } vCurrent)
                vCurrent.SetAsPlayer();
        }

        public void Die()
        {
            if (currentPlayerCharacter != null)
            {
                PhotonNetwork.Destroy(currentPlayerCharacter);
                currentPlayerCharacter = null;
            }

            Timer respawnTimer = new Timer(TimerType.Seconds, 1);
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

        public void OnHostStateChange()
        {
        }

        public void EndGame(Team teamWon)
        {
            pv.RPC("RPCEndGame", RpcTarget.Others, teamWon);
        }

        #endregion

        #region Out

        public static PlayerManager GetManagerByPhotonOwner(Player owner)
        {
            return allPlayerManagers.First(pm => pm.GetPhotonView().Owner.Equals(owner));
        }

        #endregion

        #region Internal

        #region Input

        private void OnPauseUpdate()
        {
            if (team == Team.None) return;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            HostManager.instance.SetTeam(pv.Owner.UserId, Team.None);

            if (currentPlayerCharacter != null)
                PhotonNetwork.Destroy(currentPlayerCharacter);

            Instantiate(teamSelectUI, GameObject.Find("Canvas").transform);
        }

        #endregion

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

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCEndGame(Team teamWon)
        {
            if (!pv.IsMine) return;

            InputManager.enable = false;

            bool won = teamWon == team;

            VictoryScreenUI.instance.DisplayMessage(won);
        }

        #endregion

        #endregion
    }
}