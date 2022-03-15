#region Packages

using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Character
{
    public class VisualManager : MonoBehaviour
    {
        #region Values

        [SerializeField] private PhotonView pv;
        [SerializeField] private GameObject playerVisual, nonPlayerVisual;

        #endregion

        #region Build In States

        private void Awake()
        {
            playerVisual.SetActive(pv.IsMine);
            nonPlayerVisual.SetActive(!pv.IsMine);
        }

        #endregion

        #region In

        public void SetAsNonPlayer()
        {
            playerVisual.SetActive(false);
            nonPlayerVisual.SetActive(true);
        }
        
        public void SetAsPlayer()
        {
            playerVisual.SetActive(true);
            nonPlayerVisual.SetActive(false);
        }

        #endregion
    }
}