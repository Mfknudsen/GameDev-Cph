#region Packages

using GameDev.Weapons;
using TMPro;
using UnityEngine;

#endregion

namespace GameDev.UI.FPS
{
    public class WeaponUI : MonoBehaviour
    {
        #region Values

        [SerializeField] private TextMeshProUGUI ammoCountText, weaponNameText;

        private Weapon toDisplay;

        #endregion

        #region Build In States

        private void Update()
        {
            if (toDisplay == null) return;

            ammoCountText.text = toDisplay.GetCurrentMagCount() + " / " + toDisplay.GetMaxMagCount();
        }

        #endregion

        #region Setters

        public void SetWeaponToDisplay(Weapon set)
        {
            toDisplay = set;

            if (weaponNameText != null && set != null)
                weaponNameText.text = set.name;
        }

        #endregion
    }
}