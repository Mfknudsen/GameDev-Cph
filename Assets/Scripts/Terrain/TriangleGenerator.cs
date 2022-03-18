using System.Collections.Generic;

namespace GameDev.Terrain
{
    public static class TriangleGenerator
    {
        public static int[] GenerateFromCube(CreepPoint origin, CreepPoint[] neighbors)
        {
            List<int> result = new List<int>();

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[2].vertIndex, neighbors[1].vertIndex}))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[2].vertIndex);
                result.Add(neighbors[1].vertIndex);
            }

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[0].vertIndex, neighbors[2].vertIndex}))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[2].vertIndex);
            }

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[3].vertIndex, neighbors[4].vertIndex}))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[5].vertIndex, neighbors[4].vertIndex}) &&
                neighbors[3].vertIndex.Equals(-1))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[6].vertIndex, neighbors[4].vertIndex}) &&
                (neighbors[3].vertIndex < 0 && neighbors[5].vertIndex < 0))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[0].vertIndex, neighbors[4].vertIndex}))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[0].vertIndex);
            }

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[3].vertIndex, neighbors[5].vertIndex}))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[1].vertIndex, neighbors[5].vertIndex}))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[5].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[3].vertIndex, neighbors[4].vertIndex, neighbors[5].vertIndex}))
            {
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[4].vertIndex, neighbors[6].vertIndex, neighbors[5].vertIndex}))
            {
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[4].vertIndex, neighbors[6].vertIndex, neighbors[2].vertIndex}))
            {
                result.Add(neighbors[2].vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[4].vertIndex, neighbors[2].vertIndex, neighbors[0].vertIndex}))
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[2].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[1].vertIndex, neighbors[5].vertIndex, neighbors[6].vertIndex}))
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[5].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[1].vertIndex, neighbors[2].vertIndex, neighbors[6].vertIndex}))
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[2].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[2].vertIndex, neighbors[4].vertIndex}) &&
                neighbors[0].vertIndex < 0)
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[2].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[1].vertIndex, neighbors[0].vertIndex, neighbors[6].vertIndex}) &&
                neighbors[2].vertIndex < 0)
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[0].vertIndex, neighbors[4].vertIndex, neighbors[6].vertIndex}))
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[0].vertIndex, neighbors[1].vertIndex}))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[1].vertIndex);
            }

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[1].vertIndex, neighbors[3].vertIndex}))
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[1].vertIndex, neighbors[3].vertIndex, neighbors[6].vertIndex}) &&
                neighbors[5].vertIndex < 0)
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[3].vertIndex, neighbors[4].vertIndex, neighbors[6].vertIndex}) &&
                neighbors[5].vertIndex < 0)
            {
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[3].vertIndex, neighbors[6].vertIndex}) &&
                neighbors[1].vertIndex < 0)
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }

            if (ValidTriangle(new[] {origin.vertIndex, neighbors[0].vertIndex, neighbors[6].vertIndex}) &&
                neighbors[1].vertIndex < 0)
            {
                result.Add(origin.vertIndex);
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[1].vertIndex, neighbors[0].vertIndex, neighbors[4].vertIndex}) &&
                neighbors[3].vertIndex < 0 && origin.vertIndex < 0)
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[5].vertIndex, neighbors[1].vertIndex, neighbors[4].vertIndex}) &&
                neighbors[3].vertIndex < 0 && origin.vertIndex < 0)
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[0].vertIndex, neighbors[1].vertIndex, neighbors[2].vertIndex}) &&
                origin.vertIndex < 0)
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[2].vertIndex);
                result.Add(neighbors[1].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[1].vertIndex, neighbors[2].vertIndex, neighbors[4].vertIndex}) &&
                neighbors[0].vertIndex < 0)
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[4].vertIndex);
                result.Add(neighbors[2].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[0].vertIndex, neighbors[1].vertIndex, neighbors[3].vertIndex}) &&
                origin.vertIndex < 0)
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[5].vertIndex, neighbors[1].vertIndex, neighbors[3].vertIndex}) &&
                origin.vertIndex < 0)
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[3].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[0].vertIndex, neighbors[4].vertIndex, neighbors[3].vertIndex}) &&
                origin.vertIndex < 0)
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[4].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[3].vertIndex, neighbors[5].vertIndex, neighbors[6].vertIndex}) &&
                neighbors[4].vertIndex < 0)
            {
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[5].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[0].vertIndex, neighbors[2].vertIndex, neighbors[6].vertIndex}) &&
                neighbors[4].vertIndex < 0)
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[6].vertIndex);
                result.Add(neighbors[2].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[0].vertIndex, neighbors[3].vertIndex, neighbors[6].vertIndex}) &&
                neighbors[4].vertIndex < 0)
            {
                result.Add(neighbors[0].vertIndex);
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[2].vertIndex, neighbors[3].vertIndex, neighbors[6].vertIndex}) &&
                neighbors[4].vertIndex < 0 && neighbors[0].vertIndex < 0)
            {
                result.Add(neighbors[2].vertIndex);
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[6].vertIndex);
            }

            if (ValidTriangle(new[] {neighbors[1].vertIndex, neighbors[2].vertIndex, neighbors[3].vertIndex}) &&
                origin.vertIndex < 0 && neighbors[0].vertIndex < 0)
            {
                result.Add(neighbors[1].vertIndex);
                result.Add(neighbors[3].vertIndex);
                result.Add(neighbors[2].vertIndex);
            }

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
    }
}