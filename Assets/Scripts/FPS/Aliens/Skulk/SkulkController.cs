#region Packages

using System.Collections;
using System.Collections.Generic;
using GameDev.FPS;
using UnityEngine;

#endregion

namespace GameDev
{
    public class SkulkController : FPSController
    {
        #region Internal

        protected override void Move()
        {
            base.Move();
        }

        protected override void Rotate()
        {
            base.Rotate();
        }

        private void DetectClimbableSurface()
        {
        }

        #endregion
    }
}