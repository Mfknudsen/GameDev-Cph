#region Packages

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace GameDev.UI.RTS.Grid
{
    public class GridMenu : MonoBehaviour
    {
        #region Values

        private GridItem[,] gridList;

        #endregion

        #region Build In States

        private void Awake()
        {
            int y = transform.childCount, x = transform.GetChild(0).childCount;
            gridList = new GridItem[y, x];

            for (int i = 0; i < y; i++)
            {
                Transform childTransform = transform.GetChild(i);

                for (int j = 0; j < childTransform.childCount; j++)
                {
                    if (j >= x) break;

                    try
                    {
                        gridList[x, y] = childTransform.GetChild(j).GetComponent<GridItem>();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        #endregion

        #region Getters

        public GridItem[,] GetItemGrid()
        {
            return gridList;
        }

        #endregion

        #region Out

        public GridItem[] GetItemsAsArray()
        {
            List<GridItem> result = new List<GridItem>();
            foreach (GridItem gridItem in gridList)
                result.Add(gridItem);

            return result.ToArray();
        }

        #endregion
    }
}