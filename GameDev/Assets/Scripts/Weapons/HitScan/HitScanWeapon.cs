#region Packages

using System;
using GameDev.Weapons.Ammo.HitScan;
using UnityEngine;

#endregion

namespace GameDev.Weapons.HitScan
{
    [Serializable]
    public abstract class HitScanWeapon : IWeapon
    {
        #region Values

        [SerializeField] protected int magMaxSize, magCurSize;

        [SerializeField] protected HitScanAmmo ammo;

        #endregion

        #region In

        public abstract void Trigger();

        public abstract void Reload();

        #endregion
    }
}