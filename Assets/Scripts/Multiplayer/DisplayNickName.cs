#region Packages

using GameDev.Character;
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

        [SerializeField] private Health health;

        private Transform objTransform;

        #endregion

        #region Build In States

        private void Start()
        {
            objTransform = transform;
        }

        private void Update()
        {
            if (PlayerManager.ownedManager == null) return;

            if (PlayerManager.ownedManager.GetCurrentPlayerCharacter() == transform.root.gameObject)
            {
                gameObject.SetActive(false);
                return;
            }

            if (Camera.main == null)
                return;

            objTransform.rotation =
                Quaternion.LookRotation(objTransform.position - Camera.main.transform.position);

            //if (photonView.IsMine) return;
            textDisplay.text = (photonView.IsMine ? photonView.Owner.NickName : "") +
                               "\n HP: " + health.GetCurrentHp() +
                               " AP: " + health.GetCurrentAp();
        }

        #endregion
    }
}