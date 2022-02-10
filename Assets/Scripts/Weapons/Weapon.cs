using System;
using GameDev.Weapons.Ammo.HitScan;
using UnityEngine;

namespace GameDev.Weapons
{
    [Serializable]
    public abstract class Weapon : MonoBehaviour, IWeapon
    {
        [SerializeField] protected float timeBetweenAttacking, timeBetweenAttacks, range;
        [SerializeField] protected bool allowButtonHold;
        protected bool attacking;

        public abstract void Trigger();

        public abstract void Reload();

        public Camera firstPersonCam;
        public Transform attackPoint;
        public RaycastHit rayHit;
        public LayerMask whatIsEnemy;
    }
}