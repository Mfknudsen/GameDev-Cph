#region Packages

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace GameDev.Terrain
{
    [Serializable]
    public class CreepPoint
    {
        #region Values

        public Vector3 worldPosition;

        public Vector3Int index;

        private List<CreepPoint> neighbors;

        public bool active;

        public List<int> spreadStrength;

        public float spread;

        public CreepPoint(Vector3Int index, Vector3 worldPosition)
        {
            this.index = index;
            this.worldPosition = worldPosition;

            neighbors = new List<CreepPoint>();
        }

        #endregion

        #region Getters

        public CreepPoint[] GetNeighbors()
        {
            return neighbors.ToArray();
        }

        public int GetHighestSpreadStrength()
        {
            return spreadStrength.OrderBy(s => s).First();
        }

        #endregion

        #region Setters

        public void SetNeighbors(CreepPoint[] points)
        {
            neighbors.AddRange(points);
        }

        #endregion
    }
}