#region Packages

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = Unity.Mathematics.Random;

#endregion

namespace GameDev.Terrain
{
    [Serializable]
    public class CreepPoint
    {
        #region Values

        public Vector3 worldPosition, normal;

        public Vector3Int index;

        private List<CreepPoint> neighbors = new List<CreepPoint>(),
            connectedNeighbors = new List<CreepPoint>();

        public bool active;

        public List<int> spreadStrength;

        public int vertIndex = -1;

        public List<Cube> cubesAffected = new List<Cube>();

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
            
            connectedNeighbors.Add(this);
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

        public CreepPoint[] GetConnectedNeighbors()
        {
            return connectedNeighbors.ToArray();
        }

        #endregion

        #region Setters

        public void SetNeighbors(CreepPoint[] points)
        {
            neighbors.AddRange(points);
        }

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

        public void AddConnected(CreepPoint cp)
        {
            if (!connectedNeighbors.Contains(cp))
                connectedNeighbors.Add(cp);
        }

        #endregion
    }
}