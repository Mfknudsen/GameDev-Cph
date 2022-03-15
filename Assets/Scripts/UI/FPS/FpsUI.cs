#region Packages

using GameDev.FPS;
using GameDev.Weapons;
using UnityEngine;

#endregion

namespace GameDev.UI.FPS
{
    public class FpsUI : MonoBehaviour
    {
        #region Values

        [SerializeField] private WeaponUI weaponUI;

        #endregion

        #region Build In States

        private void Awake()
        {
            transform.parent.gameObject.GetComponent<FpsController>().SetupUI(this);
        }

        #endregion

        #region In

        public void SetWeaponToDisplay(Weapon set)
        {
            weaponUI.SetWeaponToDisplay(set);
        }

        #endregion
    }
}