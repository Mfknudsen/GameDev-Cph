#region Packages

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace GameDev.Terrain.Creep
{
    public class CreepPoint
    {
        #region Values

        public Vector3 worldPosition, normal;
        public Vector3Int index;

        private List<Vector3Int> connectedNeighbors;

        public bool active, updating, partOfMesh;

        public List<int> spreadStrength;

        public int vertIndex;

        public List<Cube> cubesAffected;

        private CreepManager manager;

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
            spread = 0f;

            connectedNeighbors.Add(this.index);
        }

        #endregion

        #region Getters

        public int GetHighestSpreadStrength()
        {
            return spreadStrength.OrderBy(s => s).First();
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
            spread = set;

            if (spread.Equals(1f))
            {
                manager.RemoveUpdatePoint(index);

                updating = false;
            }
            
            if (spread > 0 && !partOfMesh)
            {
                manager.AddActivePoint(index);

                partOfMesh = true;
            }

            if (spread > 0.5f && !updating &&
                     spreadStrength.Count > 0)
            {
                int strength = spreadStrength.OrderBy(e => e).Reverse().First();

                if (strength <= 0) return;
                
                foreach (Vector3Int connectedNeighbor in GetConnectedNeighbors())
                    manager.AddUpdatePoint(connectedNeighbor, strength);

                updating = true;
            }
            
            if (spread.Equals(0f) && set < 0)
            {
                manager.RemoveActivePoint(index);

                partOfMesh = false;
                updating = false;
            }
        }

        public void AddConnected(Vector3Int cpIndex)
        {
            if (!connectedNeighbors.Contains(cpIndex))
                connectedNeighbors.Add(cpIndex);
        }

        public void AddStrength(int set)
        {
            if(!spreadStrength.Contains(set))
                spreadStrength.Add(set);
        }

        #endregion
    }
}