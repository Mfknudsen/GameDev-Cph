#region Packages

using UnityEngine;
using UnityEngine.Events;

#endregion

namespace GameDev.UI.RTS.Grid
{
    public class GridItem : MonoBehaviour
    {
        #region Values

        public readonly UnityEvent onClickEvent = new UnityEvent();

        [SerializeField] private GameObject highlight;

        #endregion

        private void Start()
        {
            highlight.SetActive(false);
        }

        #region In

        public void ButtonClick()
        {
            onClickEvent.Invoke();
        }

        #endregion
    }
}
