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

        [SerializeField, HideInInspector] public Vector3 worldPosition, normal;
        [SerializeField, HideInInspector] public Vector3Int index;

        [SerializeField, HideInInspector] private List<Vector3Int> neighbors,
            connectedNeighbors;

        [SerializeField, HideInInspector] public bool active;

        [SerializeField, HideInInspector] public List<int> spreadStrength;

        [SerializeField, HideInInspector] public int vertIndex;

        [SerializeField, HideInInspector] public List<Cube> cubesAffected;

        [SerializeField, HideInInspector] private CreepManager manager;

        [SerializeField, HideInInspector] private bool shouldUpdate;

        [SerializeField, HideInInspector] private float spread, perlinNoise;

        #endregion

        #region Build In States

        public CreepPoint(Vector3Int index, Vector3 worldPosition, CreepManager manager, float perlinNoise)
        {
            this.index = index;
            this.worldPosition = worldPosition;
            this.manager = manager;
            this.perlinNoise = perlinNoise;

            neighbors = new List<Vector3Int>();
            connectedNeighbors = new List<Vector3Int>();
            cubesAffected = new List<Cube>();
            spreadStrength = new List<int>();

            vertIndex = -1;
            normal = Vector3.zero;
            active = false;
            shouldUpdate = false;
            spread = 0;

            connectedNeighbors.Add(this.index);
        }

        #endregion

        #region Getters

        public Vector3Int[] GetNeighbors()
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

        public Vector3Int[] GetConnectedNeighbors()
        {
            return connectedNeighbors.ToArray();
        }

        #endregion

        #region Setters

        public void SetNeighbors(Vector3Int[] points)
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
            if (!connectedNeighbors.Contains(cp.index))
                connectedNeighbors.Add(cp.index);
        }

        #endregion
    }
}