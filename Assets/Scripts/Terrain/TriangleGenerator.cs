#region Packages

using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

#endregion

namespace GameDev.Terrain
{
    public static class TriangleGenerator
    {
        public static int[] GenerateFromCube(CreepPoint[] total)
        {
            List<int> result = new List<int>();

            //Total vert indexes offset by -1
            result.AddRange(CreateTriangle(total,
                new[] { -1, 2, 1 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 0, 2 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 3, 4 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 5, 4 }, new[] { 3 }));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 6, 4 }, new[] { 3, 5 }));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 4, 0 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 5, 3 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 1, 5 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { 5, 4, 3 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { 4, 5, 6 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { 2, 4, 6 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { 0, 4, 2 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { 1, 6, 5 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { 1, 2, 6 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 4, 2 }, new[] { 0 }));
            result.AddRange(CreateTriangle(total,
                new[] { 1, 0, 6 }, new[] { 2 }));
            result.AddRange(CreateTriangle(total,
                new[] { 1, 4, 6 }, new[] { 2, 0 }));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 0, 1 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 1, 3 }, new[] { 5 }));
            result.AddRange(CreateTriangle(total,
                new[] { 1, 6, 3 }, new[] { 5 }));
            result.AddRange(CreateTriangle(total,
                new[] { 3, 6, 4 }, new[] { 5 }));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 6, 3 }, new[] { 1, 5 }));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 0, 6 }, new[] { 1, 2 }));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 1, 4 }, new[] { 3, -1 }));
            result.AddRange(CreateTriangle(total,
                new[] { 1, 5, 4 }, new[] { 3, -1 }));
            result.AddRange(CreateTriangle(total,
                new[] { 0, 2, 1 }, new[] { -1 }));
            result.AddRange(CreateTriangle(total,
                new[] { 1, 4, 2 }, new[] { 0 }));
            result.AddRange(CreateTriangle(total,
                new[] { 0, 1, 3 }, new[] { -1 }));
            result.AddRange(CreateTriangle(total,
                new[] { 1, 5, 3 }, new[] { -1 }));
            result.AddRange(CreateTriangle(total,
                new[] { 0, 3, 4 }, new[] { -1 }));
            result.AddRange(CreateTriangle(total,
                new[] { 3, 5, 6 }, new[] { 4 }));
            result.AddRange(CreateTriangle(total,
                new[] { 0, 6, 2 }, new[] { 4 }));
            result.AddRange(CreateTriangle(total,
                new[] { 0, 3, 6 }, new[] { 4 }));
            result.AddRange(CreateTriangle(total,
                new[] { 2, 3, 6 }, new[] { 4, 0 }));
            result.AddRange(CreateTriangle(total,
                new[] { 1, 3, 2 }, new[] { -1, 0 }));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 1, 6 }, new[] { 3, 5 }));
            result.AddRange(CreateTriangle(total,
                new[] { 1, 2, 5 }, new[] { 6 }));
            result.AddRange(CreateTriangle(total,
                new[] { 2, 4, 5 }, new[] { 6 }));
            result.AddRange(CreateTriangle(total,
                new[] { 2, 3, 5 }, new[] { 4, 6 }));
            result.AddRange(CreateTriangle(total,
                new[] { 2, 0, 3 }, new[] { 4, 6 }));
            result.AddRange(CreateTriangle(total,
                new[] { 0, 4, 6 }, new[] { 2 }));
            result.AddRange(CreateTriangle(total,
                new[] { 5, -1, 6 }, new[] { 1, 2 }));
            result.AddRange(CreateTriangle(total,
                new[] { 1, 2, 3 }, new[] { 5, 6 }));
            result.AddRange(CreateTriangle(total,
                new[] { 2, 4, 3 }, new[] { 5, 6 }));
            result.AddRange(CreateTriangle(total,
                new[] { 5, 2, 6 }, new[] { 1, -1 }));
            result.AddRange(CreateTriangle(total,
                new[] { 0, 2, 5 }, new[] { 1, -1 }));
            result.AddRange(CreateTriangle(total,
                new[] { 3, 0, 5 }, new[] { 1, -1 }));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 2, 5 }, new[] { 1 }));
            result.AddRange(CreateTriangle(total,
                new[] { 2, 6, 5 }, new[] { 1 }));
            result.AddRange(CreateTriangle(total,
                new[] { 0, -1, 6 }, new[] { 3, 4, 5 }));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 2, 6 }, new[] { 1, 4, 5 }));
            result.AddRange(CreateTriangle(total,
                new[] { 0, -1, 3 }, new[] { 1, 4, 5 }));
            result.AddRange(CreateTriangle(total,
                new[] { 0, 2, 3 }, new[] { 1, 1, 4, 5 }));
            result.AddRange(CreateTriangle(total,
                new[] { 2, 6, 3 }, new[] { 1, 1, 4, 5 }));
            result.AddRange(CreateTriangle(total,
                new[] { 2, -1, 5 }, new[] { 0, 3, 4, 6 }));
            result.AddRange(CreateTriangle(total,
                new[] { 2, -1,3  }, new[] { 0, 4, 5, 6 }));

            return result.ToArray();
        }

        private static bool ValidTriangle(int[] vertValues)
        {
            foreach (int vertValue in vertValues)
            {
                if (vertValue < 0)
                    return false;
            }

            return true;
        }

        private static bool ValidNormal(CreepPoint[] points)
        {
            Vector3 crossNormal, avgNormal = Vector3.zero;

            for (int i = 0; i < 3; i++)
                avgNormal += points[i].normal;
            avgNormal /= 3;

            Vector3 side1 = points[1].worldPosition - points[0].worldPosition,
                side2 = points[2].worldPosition - points[0].worldPosition;

            crossNormal = Vector3.Cross(side1, side2);

            return Vector3.Angle(crossNormal, avgNormal) < 90;
        }

        private static bool AllPointsCanConnect(CreepPoint cp1, CreepPoint cp2, CreepPoint cp3)
        {
            return cp1.GetConnectedNeighbors().Contains(cp2) &&
                   cp1.GetConnectedNeighbors().Contains(cp3) &&
                   cp2.GetConnectedNeighbors().Contains(cp3);
        }

        private static int[] CreateTriangle(CreepPoint[] points, int[] triIndex, int[] dependIndex)
        {
            foreach (int i in triIndex)
            {
                if (points[i + 1] == null || !points[i + 1].active)
                    return new int[0];
            }

            if (dependIndex != null)
            {
                foreach (int i in dependIndex)
                {
                    if (points[i + 1]?.vertIndex >= 0)
                        return new int[0];
                }
            }

            List<int> result = new List<int>();

            CreepPoint cp1 = points[triIndex[0] + 1], cp2 = points[triIndex[1] + 1], cp3 = points[triIndex[2] + 1];

            if (!AllPointsCanConnect(cp1, cp2, cp3))
                return new int[0];

            if (ValidNormal(new[] { cp1, cp2, cp3 }) &&
                ValidTriangle(new[] { cp1.vertIndex, cp2.vertIndex, cp3.vertIndex }))
            {
                result.Add(cp1.vertIndex);
                result.Add(cp2.vertIndex);
                result.Add(cp3.vertIndex);
            }

            return result.ToArray();
        }
    }
}