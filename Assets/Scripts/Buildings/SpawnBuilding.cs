#region Packages

using System.Collections;
using GameDev.Multiplayer;
using GameDev.UI.RTS.Grid;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Buildings
{
    public sealed class SpawnBuilding : RestrictedBuilding
    {
        #region Values

        [SerializeField] private GameObject characterPrefab;

        [SerializeField] private bool instantSpawn, destroyOnSpawn;

        [SerializeField] private Transform spawnTransform;

        #endregion

        #region Build In States

        protected override void OnInstantiatedStart()
        {
            base.OnInstantiatedStart();

            foreach (PlayerManager playerManager in FindObjectsOfType<PlayerManager>())
            {
                playerManager.spawnPoints.Add(this);
            }
        }

        #endregion

        #region In

        public void SpawnController(PlayerManager playerManager)
        {
            StartCoroutine(Spawning(playerManager));
        }

        #endregion

        #region Internal

        private IEnumerator Spawning(PlayerManager playerManager)
        {
            yield return new WaitForSeconds(0.5f);

            if (!instantSpawn)
            {
                //Player Spawning Animation
            }
            
            playerManager.SwitchController(
                PlayerManager.CreateController(
                    characterPrefab, 
                    spawnTransform.position, 
                    spawnTransform.rotation));

            if (destroyOnSpawn) PhotonNetwork.Destroy(gameObject);
        }

        protected override void AddToActionMenu(GridMenu actionMenu)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}