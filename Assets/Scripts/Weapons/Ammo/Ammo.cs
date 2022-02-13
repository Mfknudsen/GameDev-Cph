#region Packages

using System;
using UnityEngine;

#endregion

namespace GameDev.Weapons.Ammo
{
    #region Enums

    public enum DamageType
    {
        Normal,
        Light,
        Heavy
    }

    public enum SpecialDamageType
    {
        Biological,
        Corrode,
        Flame,
        Gas,
        NerveGas,
        Puncture,
        Splash,
        Structural,
        GrenadeLauncher
    }

    #endregion

    [Serializable]
    public abstract class Ammo : ScriptableObject
    {
        #region Values

        [SerializeField] private float damage;
        [SerializeField] private DamageType damageType;
        [SerializeField] private SpecialDamageType specialDamageType;

        #endregion

        #region Getters

        public float GetDamage()
        {
            return damage;
        }

        public DamageType GetDamageType()
        {
            return damageType;
        }

        public SpecialDamageType GetSpecialDamageType()
        {
            return specialDamageType;
        }

        #endregion
    }
}