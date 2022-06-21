#region Packages

using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

namespace GameDev.Multiplayer
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        #region Values

        public static RoomManager instance;

        [SerializeField] private GameObject playerManagerPrefab, hostManagerPrefab;

        #endregion

        #region Build In States

        private void Awake()
        {
            if (instance)
                Destroy(gameObject);

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void OnEnable()
        {
            base.OnEnable();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #endregion

        #region Internal

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == "Start") return;

            PhotonNetwork.Instantiate(
                PhotonNetwork.IsMasterClient ? hostManagerPrefab.name : playerManagerPrefab.name,
                Vector3.zero,
                Quaternion.identity);

#if UNITY_EDITOR
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Instantiate(playerManagerPrefab.name, Vector3.zero, Quaternion.identity);
#endif
        }

        #endregion
    }
}