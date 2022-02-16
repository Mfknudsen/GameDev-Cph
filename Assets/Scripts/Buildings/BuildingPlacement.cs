#region Packages

using UnityEngine;

#endregion

namespace GameDev.Buildings
{
    public class BuildingPlacement : MonoBehaviour
    {
        #region Values

        [SerializeField] private string buildingNameAllowed;
        [SerializeField] private float snapDistance;

        #endregion

        #region Getters

        public string GetName()
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