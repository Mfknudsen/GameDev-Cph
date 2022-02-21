#region Packages

using System.Collections;
using GameDev.Multiplayer;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Buildings
{
    public sealed class SpawnBuilding : RestrictedBuilding
    {
        #region Values

        [SerializeField] private GameObject humanCharacterPrefab, alienCharacterPrefab;

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

            int i = Random.Range(0, 2);
            GameObject toSpawn = i == 1  ? humanCharacterPrefab : alienCharacterPrefab;

            playerManager.CreateController(toSpawn, spawnTransform.position, spawnTransform.rotation);

            if (destroyOnSpawn) PhotonNetwork.Destroy(gameObject);
        }

        #endregion
    }
}