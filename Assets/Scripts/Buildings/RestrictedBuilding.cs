using UnityEngine;

namespace GameDev.Buildings
{
    public class RestrictedBuilding : Building
    {
        #region Values

        [SerializeField] private string restrictedToObjectName;
        [HideInInspector] public bool canPlace;

        private bool overridePosition;

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