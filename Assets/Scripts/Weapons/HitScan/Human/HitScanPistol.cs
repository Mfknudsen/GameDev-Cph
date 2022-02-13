#region Packages

using GameDev.Character;
using UnityEngine;

#endregion

namespace GameDev.Weapons.HitScan.Human
{
    public class HitScanPistol : HitScanWeapon
    {
        public override void Trigger()
        {
            if (trigger.GetCanFire()) shooting = UnityEngine.Input.GetKey(KeyCode.Mouse0);
            else shooting = UnityEngine.Input.GetKeyDown(KeyCode.Mouse0);

            if (UnityEngine.Input.GetKeyDown(KeyCode.R) && magCurSize < magMaxSize && !reloading) Reload();

            if (shooting && !reloading && magCurSize > 0)
            {
                Shoot();
            }
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

            Vector3 direction = originPoint.transform.forward + new Vector3(spreadX, spreadY, 0);

            if (Physics.Raycast(originPoint.transform.position, direction, out RaycastHit rayHit, Mathf.Infinity,
                    hitMask))
            {
                Health health = rayHit.transform.gameObject.GetComponent<Health>();
                if (health)
                    health.ApplyDamage(
                        ammo.GetDamage(),
                        ammo.GetDamageType(),
                        ammo.GetSpecialDamageType());
            }

            magCurSize--;

            trigger.Pull();
        }

        private void Start()
        {
            magMaxSize = 25;
            magCurSize = 25;
        }

        private void Update()
        {
            Trigger();
        }
    }
}