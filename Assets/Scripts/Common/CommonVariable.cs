#region Packages

using System.Collections.Generic;

#endregion

namespace GameDev.Common
{
    public static class CommonVariable
    {
        public static List<T> MultiDimensionalToList<T>(T[,] Arr2d)
        {
            List<T> result = new List<T>();
            foreach (T t in Arr2d)
                result.Add(t);
            return result;
        }

        public static List<T> MultiDimensionalToList<T>(T[,,] Arr3d)
        {
            List<T> result = new List<T>();
            foreach (T t in Arr3d)
                result.Add(t);
            return result;
        }
    }
}