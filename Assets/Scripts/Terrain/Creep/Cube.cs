#region Packages

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace GameDev.Terrain.Creep
{
    public class Cube
    {
        #region Values

        private CreepPoint[] corners = new CreepPoint[8];

        private List<CreepPoint> toUpdate = new List<CreepPoint>();

        public List<Vector3Int> triangleIndexesOwned = new List<Vector3Int>();

        #endregion

        #region Getters

        public bool ShouldUpdate()
        {
            return toUpdate.Count >= 3;
        }

        #endregion

        #region In

        public void Replace(CreepPoint newPoint, CreepPoint oldPoint)
        {
            for (int i = 0; i < corners.Length; i++)
            {
                if (corners[i] == oldPoint)
                {
                    corners[i] = newPoint;
                    break;
                }
            }
        }

        public void AddPoint(CreepPoint point, int index)
        {
            if (index < 0 || index >= corners.Length)
                return;

            corners[index]?.cubesAffected.Remove(this);

            corners[index] = point;

            point?.cubesAffected.Add(this);
        }

        public void AddToUpdate(CreepPoint point)
        {
            if (!toUpdate.Contains(point))
                toUpdate.Add(point);
        }

        public void RemoveFromUpdate(CreepPoint point)
        {
            if (toUpdate.Contains(point))
                toUpdate.Remove(point);
        }

        #endregion

        #region Out

        public int[] Calculate(CreepPoint focus)
        {
            return TriangleGenerator.GenerateFromCube(corners, focus);
        }

        #endregion
    }
}