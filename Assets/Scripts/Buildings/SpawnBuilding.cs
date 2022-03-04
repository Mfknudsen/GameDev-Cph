#region Packages

using System.Collections;
using GameDev.Multiplayer;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Buildings
{
    public sealed class SpawnBuilding : Building
    {
        #region Values

        [SerializeField] private GameObject character;

        [SerializeField] private bool instantSpawn, destroyOnSpawn;

        [SerializeField] private Transform spawnTransform;

        #endregion

        #region Build In States

        public override void Die()
        {
        }

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

            playerManager.CreateController(character, spawnTransform.position, spawnTransform.rotation);

            if (destroyOnSpawn) PhotonNetwork.Destroy(gameObject);
        }

        #endregion
    }
}