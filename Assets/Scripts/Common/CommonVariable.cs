#region Packages

using System.Collections.Generic;

#endregion

namespace GameDev.Common
{
    public static class CommonVariable
    {
        public static List<T> MultiDimensionalToList<T>(T[,] arr2d)
        {
            List<T> result = new List<T>();
            foreach (T t in arr2d)
                result.Add(t);
            return result;
        }

        public static List<T> MultiDimensionalToList<T>(T[,,] arr3d)
        {
            List<T> result = new List<T>();
            foreach (T t in arr3d)
                result.Add(t);
            return result;
        }
    }
}