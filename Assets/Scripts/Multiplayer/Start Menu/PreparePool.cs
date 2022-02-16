#region Packages

using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Multiplayer.Start_Menu
{
    public class PreparePool : MonoBehaviour
    {
        #region Values

        [SerializeField] private List<GameObject> prefabs = new List<GameObject>();

        #endregion

        #region Build In States

        private void OnValidate()
        {
            List<string> names = new List<string>();

            foreach (GameObject prefab in prefabs)
            {
                if (names.Contains(prefab.name))
                {
                    Debug.LogError(prefab.name + " already exits in the list");
                    prefabs.Remove(prefab);
                }
                else
                    names.Add(prefab.name);
            }
        }

        private void Start()
        {
            DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
            if (pool == null || prefabs == null)
                return;

            foreach (GameObject prefab in prefabs)
                pool.ResourceCache.Add(prefab.name, prefab);
        }

        #endregion
    }
}