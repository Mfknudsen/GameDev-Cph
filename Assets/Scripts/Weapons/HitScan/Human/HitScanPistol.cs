using UnityEngine;

namespace GameDev.Weapons.HitScan.Human
{
    public class HitScanPistol : HitScanWeapon
    {
        public override void Trigger()
        {
            if (allowButtonHold) attacking = UnityEngine.Input.GetKey(KeyCode.Mouse0);
            else attacking = UnityEngine.Input.GetKeyDown(KeyCode.Mouse0);

            if (UnityEngine.Input.GetKeyDown(KeyCode.R) && magCurSize < magMaxSize && !reloading) Reload();

            if (readyToShoot && attacking && !reloading && magCurSize > 0) {
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
            readyToShoot = false;

            float spread_x = Random.Range(-spread, spread);
            float spread_y = Random.Range(-spread, spread);

            Vector3 direction = firstPersonCam.transform.forward + new Vector3(spread_x, spread_y, 0);

            if (Physics.Raycast(firstPersonCam.transform.position, direction, out rayHit, range, whatIsEnemy))
            {
                Debug.Log(rayHit.collider.name);

                // if (rayHit.collider.CompareTag("Alien"))
                //     rayHit.collider.GetComponent<Info>().TakeDamage(damage);
            }

            magCurSize--;

            Invoke("ResetShot", timeBetweenAttacking);
        }

        private void ResetShot()
        {
            readyToShoot = true;
        }

        private void Start()
        {
            magMaxSize = 25;
            magCurSize = 25;
            readyToShoot = true;
            ammo.SetDamage(12);
        }

        private void Update()
        {
            Trigger();
        }
    }
}
