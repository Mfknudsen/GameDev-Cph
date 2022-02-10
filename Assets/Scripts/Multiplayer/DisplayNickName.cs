#region Packages

using Photon.Pun;
using TMPro;
using UnityEngine;

#endregion

namespace GameDev.Multiplayer
{
    public class DisplayNickName : MonoBehaviour
    {
        #region Values

        [SerializeField] private TextMeshPro textDisplay;
        [SerializeField] private PhotonView photonView;

        private Transform objTransform;

        #endregion

        #region Build In States

        private void Start()
        {
            objTransform = transform;

            if (photonView.IsMine)
                gameObject.SetActive(false);
            else
                textDisplay.text = photonView.Owner.NickName;
        }

        private void Update()
        {
            if (Camera.main == null)
                return;

            objTransform.rotation =
                Quaternion.LookRotation(objTransform.position - Camera.main.transform.position);
        }

        #endregion
    }
}