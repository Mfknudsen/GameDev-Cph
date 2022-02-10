#region Packages

using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace GameDev.Multiplayer
{
    public class SpawnPlayer : MonoBehaviour
    {
        #region Values

        [SerializeField] private GameObject playerPrefab;

        #endregion

        #region Build In States

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;

            PhotonNetwork.Instantiate(playerPrefab.name, transform.position, quaternion.identity);
        }

        #endregion
    }
}