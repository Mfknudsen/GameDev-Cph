#region Packages

using GameDev.Common;
using GameDev.UI.RTS.Grid;
using UnityEngine;

#endregion

namespace GameDev.Buildings
{
    public class NodeBuilding : RestrictedBuilding
    {
        #region Values

        [SerializeField] private float valuePerTick, timeBetweenTick;

        #endregion

        #region Build In States

        protected override void OnLocalStart()
        {
            base.OnLocalStart();

            onBuildComplete.AddListener(() =>
            {
                Timer tickTimer = new Timer(timeBetweenTick);
                tickTimer.timerEvent.AddListener(() => { IncreaseCoin(); });
            });
        }

        #endregion

        #region In

        protected override void AddToActionMenu(GridMenu actionMenu)
        {
        }

        #endregion

        #region Internal

        private void IncreaseCoin()
        {
            
            
            Timer tickTimer = new Timer(timeBetweenTick);
            tickTimer.timerEvent.AddListener(() => { IncreaseCoin(); });
        }

        #endregion
    }
}