#region Packages

using UnityEngine;

#endregion

namespace GameDev.Multiplayer
{
    public class SpawnPlayer : MonoBehaviour
    {
        #region Values

        [SerializeField] private GameObject playerPrefab;

        #endregion

        #region In

        public void Spawn(PlayerManager manager)
        {
            Transform trans = transform;
            manager.SwitchController(
                PlayerManager.CreateController(playerPrefab, trans.position, trans.rotation));
        }

        #endregion
    }
}