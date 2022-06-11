#region Packages

using GameDev.Character;
using UnityEngine;

#endregion

namespace GameDev.Weapons.HitScan.Human
{
    public class HitScanPistol : HitScanWeapon
    {
        #region Internal

        protected override void Trigger()
        {
            float spreadX = Random.Range(-spread, spread);
            float spreadY = Random.Range(-spread, spread);

            Ray ray = new Ray(origin.position, origin.forward + new Vector3(spreadX, spreadY, 0));
            if (Physics.Raycast(ray, out RaycastHit rayHit, 50, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (rayHit.transform.root.gameObject.GetComponent<Health>() is { } health)
                {
                    health.ApplyDamage(
                        ammunition.GetDamage(),
                        ammunition.GetDamageType(),
                        ammunition.GetSpecialDamageType(),
                        team);
                }
            }

            magCurSize -= ammoPerShot;

            trigger.Pull();

            if (magCurSize == 0)
                Reload();
        }

        #endregion
    }
}