#region Packages

using UnityEngine;

#endregion

namespace GameDev.Buildings
{
    public abstract class RestrictedBuilding : Building
    {
        #region Values

        [SerializeField] private BuildingType buildingType;
        private bool canPlace;

        #endregion

        #region Getters

        public BuildingType GetBuildingType()
        {
            return buildingType;
        }

        #endregion

        #region Setters

        public void SetCanPlace(bool set)
        {
            canPlace = set;
        }

        #endregion

        #region Out

        public override bool CanBePlaced()
        {
            return canPlace;
        }

        #endregion
    }
}