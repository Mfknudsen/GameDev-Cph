#region Packages

using System;
using ExitGames.Client.Photon;
using GameDev.Common;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Weapons
{
    [Serializable]
    public abstract class Gun : Weapon
    {
        #region Values

        [SerializeField] protected float spread, reloadTime;

        #endregion

        #region Internal

        protected override void Reload()
        {
            SwitchReload(true);

            magCurSize = 0;
            Timer reloadTimer = new Timer(TimerType.Seconds, reloadTime);
            reloadTimer.timerEvent.AddListener(() =>
            {
                magCurSize = magMaxSize;

                SwitchReload(false);
            });
        }

        protected void SwitchReload(bool set)
        {
            reloading = set;

            Hashtable hash = new Hashtable();
            hash.Add("Reloading", reloading);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        #endregion
    }
}