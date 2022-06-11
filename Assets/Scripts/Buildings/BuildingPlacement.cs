#region Packages

using UnityEngine;

#endregion

namespace GameDev.Buildings
{
    #region Enums

    public enum BuildingType
    {
        Node,
        HQ
    }

    #endregion

    public class BuildingPlacement : MonoBehaviour
    {
        #region Values

        [SerializeField] private BuildingType buildingNameAllowed;
        [SerializeField] private float snapDistance;

        #endregion

        #region Getters

        public BuildingType GetTypeAllowed()
        {
            return buildingNameAllowed;
        }

        public float GetDistance()
        {
            return snapDistance;
        }

        #endregion
    }
}