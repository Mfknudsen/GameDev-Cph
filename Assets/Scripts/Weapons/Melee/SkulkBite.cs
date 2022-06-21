#region Packages

using System.Linq;
using GameDev.Character;
using UnityEngine;

#endregion

namespace GameDev.Weapons.Melee
{
    public class SkulkBite : Weapon
    {
        #region Values

        [Space] [Header("Bite")] [SerializeField]
        private Vector3 boxSize;

        [SerializeField] private LayerMask hitMask;

        #endregion

        #region Build In States

        private void OnDrawGizmos()
        {
            Gizmos.matrix = origin.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }

        #endregion

        #region Internal

        protected override void Reload()
        {
        }

        protected override void Trigger()
        {
            TryGetToRoot(BiteHit())?
                .ApplyDamage(
                    ammunition.GetDamage(),
                    ammunition.GetDamageType(),
                    ammunition.GetSpecialDamageType(),
                    team);
        }

        private GameObject BiteHit()
        {
            GameObject closestHit = null;
            float dist = 0;
            // ReSharper disable once Unity.PreferNonAllocApi
            Collider[] colliders = Physics.OverlapBox(
                origin.position,
                boxSize,
                origin.rotation,
                hitMask,
                QueryTriggerInteraction.Ignore);

            foreach (GameObject obj in colliders
                         .Select(c => c.gameObject)
                         .Where(o => o.transform.root.gameObject != transform.root.gameObject))
            {
                float newDist = Vector3.Distance(origin.position, obj.transform.position);
                if (closestHit == null || newDist < dist)
                {
                    closestHit = obj;
                    dist = newDist;
                }
            }

            return closestHit;
        }

        private static Health TryGetToRoot(GameObject obj)
        {
            if (obj == null) return null;

            Health result = obj.GetComponent<Health>();

            if (result != null)
                return result;

            Transform p = obj.transform.parent;

            return p == null ? null : TryGetToRoot(p.gameObject);
        }

        #endregion
    }
}