#region Packages

using GameDev.Character;
using TMPro;
using UnityEngine;

#endregion

namespace GameDev.UI.FPS
{
    public class HealthUI : MonoBehaviour
    {
        #region Values

        [SerializeField] private TextMeshProUGUI hpText, apText;

        private Health toDisplay;

        #endregion

        #region Build In States

        private void Update()
        {
            if (toDisplay == null)
                return;

            if (hpText != null)
                hpText.text = "HP / " + toDisplay.GetCurrentHp();
            if (apText != null)
                apText.text = "AP / " + toDisplay.GetCurrentAp();
        }

        #endregion

        #region Setters

        public void SetHealthToDisplay(Health set)
        {
            toDisplay = set;
        }

        #endregion
    }
}