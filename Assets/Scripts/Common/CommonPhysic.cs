#region Packages

using UnityEngine;

#endregion

namespace GameDev.Common
{
    public static class CommonPhysic
    {
        public static Collider GetNearestSurfaceBySphere(Vector3 originPosition, float checkDistance,
            LayerMask mask = default)
        {
            // ReSharper disable once Unity.PreferNonAllocApi
            Collider[] colliders = Physics.OverlapSphere(
                originPosition,
                checkDistance,
                mask);

            Collider closest = null;
            if (colliders.Length > 0)
            {
                Vector3 currentPosition = originPosition;
                float dist = 0;

                foreach (Collider c in colliders)
                {
                    //Constant warning from this function.
                    //Dont seem to be a problem but cant figure out how to disable.
                    Vector3 pos = c.ClosestPoint(currentPosition);

                    float newDist = Vector3.Distance(pos, currentPosition);

                    if (newDist == 0) continue;

                    if (closest == null)
                    {
                        closest = c;
                        dist = Vector3.Distance(pos, currentPosition);
                        continue;
                    }

                    if (newDist < dist)
                    {
                        closest = c;
                        dist = newDist;
                    }
                }
            }

            return closest;
        }

        public static T HitComponent<T>(RaycastHit hit)
        {
            return hit.collider.GetComponent<T>();
        }
    }
}