#region Packages

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace GameDev.Terrain
{
    public static class TriangleGenerator
    {
        public static int[] GenerateFromCube(CreepPoint origin, CreepPoint[] neighbors)
        {
            List<int> result = new List<int>();

            //To call origin from total in the function CreateTriangle use -1
            CreepPoint[] total = new[] { origin };
            total = total.Concat(neighbors).ToArray();
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
                new[] { -1, 3, 5 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { -1, 1, 5 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { 5, 4, 3 }, null));
            result.AddRange(CreateTriangle(total,
                new[] { 4, 6, 5 }, null));
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
                new[] { 1, 4, 6 }, null));
            result.AddRange(CreateTriangle(total, 
                new[] { -1, 0, 1 }, null));
            result.AddRange(CreateTriangle(total, 
                new[] { -1, 1, 3 }, null));
            result.AddRange(CreateTriangle(total, 
                new[] { 1, 6, 3 }, new[] { 5 }));
            result.AddRange(CreateTriangle(total, 
                new[] { 3, 6, 4 }, new[] { 5 }));
            result.AddRange(CreateTriangle(total, 
                new[] { -1, 6, 3 }, new[] { 1 }));
            result.AddRange(CreateTriangle(total, 
                new[] { -1, 0, 6 }, new[] { 1 }));
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
            return result.ToArray();

            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[2].vertIndex, neighbors[1].vertIndex }) &&
                ValidNormal(new[] { origin, neighbors[2], neighbors[1] }))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[2].vertIndex);
                result.Add(neighbors[1].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[0].vertIndex, neighbors[2].vertIndex }) &&
                ValidNormal(new[] { origin, neighbors[0], neighbors[2] }))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[2].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[3].vertIndex, neighbors[4].vertIndex }) &&
                ValidNormal(new[] { origin, neighbors[3], neighbors[4] }))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[5].vertIndex, neighbors[4].vertIndex }) &&
                neighbors[3].vertIndex.Equals(-1) &&
                ValidNormal(new[] { origin, neighbors[5], neighbors[4] }))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[6].vertIndex, neighbors[4].vertIndex }) &&
                (neighbors[3].vertIndex < 0 && neighbors[5].vertIndex < 0) &&
                ValidNormal(new[] { origin, neighbors[6], neighbors[4] }))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[0].vertIndex, neighbors[4].vertIndex }))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[0].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[3].vertIndex, neighbors[5].vertIndex }))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[1].vertIndex, neighbors[5].vertIndex }))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[5].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[3].vertIndex, neighbors[4].vertIndex, neighbors[5].vertIndex }))
            {
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[4].vertIndex, neighbors[6].vertIndex, neighbors[5].vertIndex }))
            {
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[4].vertIndex, neighbors[6].vertIndex, neighbors[2].vertIndex }))
            {
                result.Add(neighbors[2].vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[4].vertIndex, neighbors[2].vertIndex, neighbors[0].vertIndex }))
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[2].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[5].vertIndex, neighbors[6].vertIndex }))
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[5].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[2].vertIndex, neighbors[6].vertIndex }))
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[2].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[2].vertIndex, neighbors[4].vertIndex }) &&
                neighbors[0].vertIndex < 0)
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[2].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[0].vertIndex, neighbors[6].vertIndex }) &&
                neighbors[2].vertIndex < 0)
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[0].vertIndex, neighbors[4].vertIndex, neighbors[6].vertIndex }))
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[0].vertIndex, neighbors[1].vertIndex }))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[1].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[1].vertIndex, neighbors[3].vertIndex }))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[3].vertIndex, neighbors[6].vertIndex }) &&
                neighbors[5].vertIndex < 0)
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[3].vertIndex, neighbors[4].vertIndex, neighbors[6].vertIndex }) &&
                neighbors[5].vertIndex < 0)
            {
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[3].vertIndex, neighbors[6].vertIndex }) &&
                neighbors[1].vertIndex < 0)
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { origin.vertIndex, neighbors[0].vertIndex, neighbors[6].vertIndex }) &&
                neighbors[1].vertIndex < 0)
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[0].vertIndex, neighbors[4].vertIndex }) &&
                neighbors[3].vertIndex < 0 && origin.vertIndex < 0)
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[5].vertIndex, neighbors[1].vertIndex, neighbors[4].vertIndex }) &&
                neighbors[3].vertIndex < 0 && origin.vertIndex < 0)
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[0].vertIndex, neighbors[1].vertIndex, neighbors[2].vertIndex }) &&
                origin.vertIndex < 0)
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[2].vertIndex);
                result.Add(neighbors[1].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[2].vertIndex, neighbors[4].vertIndex }) &&
                neighbors[0].vertIndex < 0)
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[2].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[0].vertIndex, neighbors[1].vertIndex, neighbors[3].vertIndex }) &&
                origin.vertIndex < 0)
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[5].vertIndex, neighbors[1].vertIndex, neighbors[3].vertIndex }) &&
                origin.vertIndex < 0)
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[0].vertIndex, neighbors[4].vertIndex, neighbors[3].vertIndex }) &&
                origin.vertIndex < 0)
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[3].vertIndex, neighbors[5].vertIndex, neighbors[6].vertIndex }) &&
                neighbors[4].vertIndex < 0)
            {
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[0].vertIndex, neighbors[2].vertIndex, neighbors[6].vertIndex }) &&
                neighbors[4].vertIndex < 0)
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[2].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[0].vertIndex, neighbors[3].vertIndex, neighbors[6].vertIndex }) &&
                neighbors[4].vertIndex < 0)
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[2].vertIndex, neighbors[3].vertIndex, neighbors[6].vertIndex }) &&
                neighbors[4].vertIndex < 0 && neighbors[0].vertIndex < 0)
            {
                result.Add(neighbors[2].vertIndex);
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }
            */
            /*
            if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[2].vertIndex, neighbors[3].vertIndex }) &&
                origin.vertIndex < 0 && neighbors[0].vertIndex < 0)
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[2].vertIndex);
            }
            */
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

        private static int[] CreateTriangle(CreepPoint[] points, int[] triIndex, int[] dependIndex)
        {
            if (dependIndex != null)
            {
                foreach (int i in dependIndex)
                {
                    if (points[i + 1].vertIndex < 0)
                        return new int[0];
                }
            }

            List<int> result = new List<int>();

            CreepPoint cp1 = points[triIndex[0] + 1], cp2 = points[triIndex[1] + 1], cp3 = points[triIndex[2] + 1];

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