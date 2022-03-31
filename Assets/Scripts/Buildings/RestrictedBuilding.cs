#region Packages

using UnityEngine;

#endregion

namespace GameDev.Buildings
{
    public abstract class RestrictedBuilding : Building
    {
        #region Values

        [SerializeField] private string restrictedToObjectName;

        #endregion

        #region Getters

        public string GetRestrictedName()
        {
            return restrictedToObjectName;
        }

        #endregion

        #region Out

        public override bool CanBePlaced()
        {
            return true;
        }

        #endregion

        #region Internal

        #endregion
    }
}