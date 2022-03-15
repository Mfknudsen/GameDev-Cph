#region Packages

using System;
using GameDev.Common;
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
            reloading = true;
            magCurSize = 0;
            Timer reloadTimer = new Timer(reloadTime);
            reloadTimer.timerEvent.AddListener(() =>
            {
                magCurSize = magMaxSize;
                reloading = false;
            });
        }

        #endregion
    }
}