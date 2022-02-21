#region Packages

using System.Collections.Generic;
using GameDev.Character;
using UnityEngine;

#endregion

namespace GameDev.Weapons.HitScan.Human
{
    public class HitScanPistol : HitScanWeapon
    {
        private List<Ray> debugRays = new List<Ray>();

        public override void Trigger()
        {
            if (trigger.GetCanFire()) shooting = UnityEngine.Input.GetKey(KeyCode.Mouse0);
            else shooting = UnityEngine.Input.GetKeyDown(KeyCode.Mouse0);

            if (UnityEngine.Input.GetKeyDown(KeyCode.R) && magCurSize < magMaxSize && !reloading) Reload();

            if (shooting && !reloading && magCurSize > 0)
                Shoot();
        }

        public override void Reload()
        {
            reloading = true;
            Invoke("ReloadFinished", reloadTime);
        }

        private void ReloadFinished()
        {
            magCurSize = magMaxSize;
            reloading = false;
        }

        private void Shoot()
        {
            float spreadX = Random.Range(-spread, spread);
            float spreadY = Random.Range(-spread, spread);

            Ray ray = new Ray(origin.position, origin.forward + new Vector3(spreadX, spreadY, 0));
            debugRays.Add(ray);
            if (Physics.Raycast(ray, out RaycastHit rayHit, 50, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (rayHit.transform.root.gameObject.GetComponent<Health>() is { } health)
                {
                    health.ApplyDamage(
                        ammo.GetDamage(),
                        ammo.GetDamageType(),
                        ammo.GetSpecialDamageType());
                }
            }

            //magCurSize--;

            trigger.Pull();
        }

        private void Start()
        {
            magMaxSize = 25;
            magCurSize = 25;
        }

        protected override void Update()
        {
            if (pv.IsMine)
                Trigger();

            if (debugRays.Count > 10)
                debugRays.RemoveAt(0);

            debugRays.ForEach(r => Debug.DrawRay(r.origin, r.direction, Color.red));
        }
    }
}