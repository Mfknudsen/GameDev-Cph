#region Packages

using System;
using GameDev.Character;
using UnityEngine;

#endregion

namespace GameDev.Weapons.HitScan
{
    [Serializable]
    public abstract class HitScanWeapon : Gun
    {
        #region Values

        [Space] [Header("Shooting Ray")] [SerializeField]
        protected Transform originPoint;

        [SerializeField] protected LayerMask hitMask;

        #endregion

        #region Internal

        protected void ShootRay()
        {
            Ray ray = new Ray(originPoint.position, originPoint.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hitMask))
            {
                GameObject hitObject = hit.transform.gameObject;
                if (pv.IsMine)
                {
                    Health health = hitObject.GetComponent<Health>();
                    if (health)
                        health.ApplyDamage(
                            ammunition.GetDamage(),
                            ammunition.GetDamageType(),
                            ammunition.GetSpecialDamageType(),
                            team);
                }
            }
        }

        #endregion
    }
}