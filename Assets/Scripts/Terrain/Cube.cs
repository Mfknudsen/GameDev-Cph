#region Packages

using System;

#endregion

namespace GameDev.Terrain
{
    public class Cube
    {
        #region Values

        private CreepPoint[] corners = new CreepPoint[8];

        public bool toUpdate;

        #endregion

        #region Build In States

        public Cube(CreepPoint[] points)
        {
           
        }

        #endregion
    }
}