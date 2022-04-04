#region Packages

using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

#endregion

namespace GameDev.Multiplayer
{
    #region Enums

    public enum PlayerStat
    {
        WeaponLevel,
        ArmorLevel
    }

    #endregion

    public class PlayerStats : MonoBehaviour
    {
        #region Values

        public UnityEvent onStatsChangeEvent;

        [SerializeField] private PhotonView pv;

        [SerializeField]
        private SerializedDictionary<string, object> stats = new SerializedDictionary<string, object>();

        #endregion

        #region Build In States

        private void OnEnable()
        {
            if (!pv.IsMine)
                return;

            Hashtable hostHash = PhotonNetwork.MasterClient.CustomProperties;
            foreach (string key in hostHash.Keys
                         .Where(k => ((string)k).Contains("Stat")))
                stats.Add(key, hostHash[key]);
        }

        #endregion

        #region Out

        public object GetStatValueByKey(PlayerStat stat)
        {
            string key = "Stat" + stat.ToString();

            if (!stats.ContainsKey(key))
                return null;

            return stats[key];
        }

        #endregion
    }
}