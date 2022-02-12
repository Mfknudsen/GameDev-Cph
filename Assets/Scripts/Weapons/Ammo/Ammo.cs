#region Packages

using System;
using UnityEngine;

#endregion

namespace GameDev.Weapons.Ammo
{
    [Serializable]
    public abstract class Ammo : ScriptableObject
    {
        [SerializeField] private float damage;

        public float GetDamage()
        {
            return damage;
        }
        public void SetDamage(float _damage)
        {
            damage = _damage;
        }
    }
}