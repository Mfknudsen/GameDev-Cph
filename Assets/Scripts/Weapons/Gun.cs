using System;
using UnityEngine;

namespace GameDev.Weapons
{
    [Serializable]
    public abstract class Gun : Weapon
    {
        [SerializeField] protected int magMaxSize, magCurSize, bulletsPerTap;
        [SerializeField] protected float spread, reloadTime;
        protected bool reloading, readyToShoot;
    }
}