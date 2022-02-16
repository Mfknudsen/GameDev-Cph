#region Packages

using System.Collections.Generic;
using System.Linq;
using GameDev.RTS;
using GameDev.UI.RTS.Grid;
using UnityEngine;

#endregion

namespace GameDev.UI.RTS
{
    public class RtsUI : MonoBehaviour
    {
        #region Values

        [SerializeField] private GridMenu selectedMenu, actionMenu;

        private RtsController rtsController;
        
        private List<IUnit> highlightUnit = new List<IUnit>();

        #endregion

        #region In

        public void Setup(RtsController controller)
        {
            rtsController = controller;
        }
        
        public void UpdateSelected(IUnit[] units)
        {
            GridItem[] items = selectedMenu.GetItemsAsArray();

            for (int i = 0; i < units.Length && i < items.Length; i++)
            {
                GridItem item = items[i];
                IUnit unit = units[i];
                
                if(!highlightUnit.Any(u => u.GetType() == unit.GetType()))
                    highlightUnit.Add(unit);
            }
        }

        #region Buttons

        public void Eject()
        {
            rtsController.GetCommandBuilding().TriggerInteraction();
        }

        #endregion

        #endregion
    }
}