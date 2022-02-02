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
    }
}