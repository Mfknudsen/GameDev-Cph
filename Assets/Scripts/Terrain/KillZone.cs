#region Packages

using GameDev.Character;
using GameDev.Multiplayer;
using GameDev.Weapons.Ammo;
using UnityEngine;

#endregion

namespace GameDev.Terrain
{
    public class KillZone : MonoBehaviour
    {
        #region Build In States

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.GetComponent<Health>() is { } h)
                h.ApplyDamage(Mathf.Infinity, DamageType.Normal, SpecialDamageType.Structural, Team.None);
        }

        #endregion
    }
}