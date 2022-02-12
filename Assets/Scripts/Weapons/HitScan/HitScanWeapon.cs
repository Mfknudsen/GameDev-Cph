#region Packages

using System;
using GameDev.Weapons.Ammo.HitScan;
using UnityEngine;

#endregion

namespace GameDev.Weapons.HitScan
{
    [Serializable]
    public abstract class HitScanWeapon : Gun
    {
        [SerializeField] protected HitScanAmmo ammo;
    }
}