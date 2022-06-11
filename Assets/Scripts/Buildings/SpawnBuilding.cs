#region Packages

using System.Collections;
using GameDev.Multiplayer;
using GameDev.UI.RTS.Grid;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Buildings
{
    public sealed class SpawnBuilding : Building
    {
        #region Values

        [SerializeField] private GameObject characterPrefab;

        [SerializeField] private bool instantSpawn, destroyOnSpawn;

        [SerializeField] private Transform spawnTransform;

        [SerializeField] private float delay;

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
                    spawnTransform
                )
            );

            if (destroyOnSpawn) PhotonNetwork.Destroy(gameObject);

            yield return new WaitForSeconds(delay);

            Vector3 floatPos = transform.position;
            Vector3Int pos = new Vector3Int((int) floatPos.x, (int) floatPos.y, (int) floatPos.z);
            HostManager.instance.SetHostState(gameObject.name + (pos.x + pos.y, pos.z), false);
        }

        protected override void AddToActionMenu(GridMenu actionMenu)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}