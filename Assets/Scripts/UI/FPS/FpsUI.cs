#region Packages

using GameDev.Character;
using GameDev.FPS;
using GameDev.Weapons;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.UI.FPS
{
    public class FpsUI : MonoBehaviour
    {
        #region Values

        [SerializeField] private WeaponUI weaponUI;
        [SerializeField] private HealthUI healthUI;

        #endregion

        #region Build In States

        private void Awake()
        {
            GameObject parent = transform.root.gameObject;
            if (parent.GetComponent<PhotonView>().IsMine)
                parent.GetComponent<FpsController>().SetupUI(this);
            else
                gameObject.SetActive(false);
        }

        #endregion

        #region In

        public void SetWeaponToDisplay(Weapon set)
        {
            if (weaponUI != null)
                weaponUI.SetWeaponToDisplay(set);
        }

        public void SetHealthToDisplay(Health set)
        {
            if (healthUI != null)
                healthUI.SetHealthToDisplay(set);
        }

        #endregion
    }
}