#region Packages

using UnityEngine;

#endregion

namespace GameDev.RTS
{
    public interface IUnit
    {
        void SetWaypoint(Vector3 waypointPos);

        void DamageIn(float damage);

        void HealIn(float heal);
    }
}