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

         public Vector3 worldPosition, normal;
      public Vector3Int index;

   private List<Vector3Int> connectedNeighbors;

         public bool active;

        public List<int> spreadStrength;

        public int vertIndex;

       public List<Cube> cubesAffected;

        private CreepManager manager;

        private bool shouldUpdate;

         private float spread, perlinNoise;

        #endregion

        #region Build In States

        public CreepPoint(Vector3Int index, Vector3 worldPosition, CreepManager manager, float perlinNoise)
        {
            this.index = index;
            this.worldPosition = worldPosition;
            this.manager = manager;
            this.perlinNoise = perlinNoise;

            connectedNeighbors = new List<Vector3Int>();
            cubesAffected = new List<Cube>();
            spreadStrength = new List<int>();

            vertIndex = -1;
            normal = Vector3.zero;
            active = false;
            shouldUpdate = false;
            spread = 0f;

            connectedNeighbors.Add(this.index);
        }

        #endregion

        #region Getters
        
        public int GetHighestSpreadStrength()
        {
            return spreadStrength.OrderBy(s => s).First();
        }

        public bool ShouldUpdate()
        {
            return shouldUpdate;
        }

        public float GetSpread()
        {
            return spread;
        }

        public float GetNoise()
        {
            return perlinNoise;
        }

        public Vector3Int[] GetConnectedNeighbors()
        {
            return connectedNeighbors.ToArray();
        }

        #endregion

        #region Setters

        public void SetSpread(float set)
        {
            if (spread == 0 && set > 0)
            {
                shouldUpdate = true;
                manager.AddPointAndUpdateTriangles(this);
            }
            else if (spread == 0 && set < 0)
            {
                shouldUpdate = false;
                manager.RemovePointAndUpdateList(this);
            }

            spread = set;
        }

        public void AddConnected(Vector3Int cpIndex)
        {
            if (!connectedNeighbors.Contains(cpIndex))
                connectedNeighbors.Add(cpIndex);
        }

        #endregion
    }
}