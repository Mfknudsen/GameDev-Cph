#region Packages

using UnityEngine;

#endregion

namespace GameDev.RTS
{
    public interface IUnit
    {
        void Select(Selector selector);
        
        void SetWaypoint(Vector3 waypointPos);
    }
}