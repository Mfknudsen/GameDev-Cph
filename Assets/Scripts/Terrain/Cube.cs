#region Packages

using System.Collections.Generic;
using System.Linq;

#endregion

namespace GameDev.Terrain
{
    public class Cube
    {
        #region Values

        private CreepPoint[] corners = new CreepPoint[8];

        private List<CreepPoint> toUpdate = new List<CreepPoint>();

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
                    corners[i] = corners.Contains(newPoint) ? null : newPoint;
                    break;
                }
            }
        }

        public void AddPoint(CreepPoint point)
        {
            if (point == null) return;

            for (int i = 0; i < corners.Length; i++)
            {
                if (corners[i] == null)
                {
                    corners[i] = point;
                    point.cubesAffected.Add(this);

                    break;
                }
            }
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

        public int[] Calculate()
        {
            return TriangleGenerator.GenerateFromCube(corners);
        }

        #endregion
    }
}