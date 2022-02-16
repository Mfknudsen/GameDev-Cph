#region Packages

using ExitGames.Client.Photon;
using GameDev.Input;
using GameDev.Interaction;
using GameDev.Multiplayer;
using GameDev.RTS;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

#endregion

namespace GameDev.Buildings
{
    public class CommandBuilding : RestrictedBuilding, IInteract
    {
        #region Values

        [SerializeField] private GameObject rtsController;

        [SerializeField] private Vector3 spawnOffset;

        private GameObject currentCommander, fpsController;

        private bool canBeTriggered = true;

        #endregion

        #region Build In States

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

            if (!pv.Owner.Equals(targetPlayer)) return;

            if (changedProps.ContainsKey("occupied"))
                canBeTriggered = !(bool) changedProps["occupied"];
        }

        #endregion

        #region In

        public void TriggerInteraction()
        {
            Hashtable hash = new Hashtable();
            if (currentCommander != null)
            {
                fpsController.GetComponent<Controller>().SetGameObjectActivePun(true);
                PhotonNetwork.Destroy(currentCommander);
                currentCommander = null;
                PlayerManager.ownedManager.SwitchCurrentController(fpsController);
                hash.Add("occupied", false);
            }
            else if (canBeTriggered)
            {
                fpsController = PlayerManager.ownedManager.GetCurrentPlayerCharacter();
                fpsController.GetComponent<Controller>().SetGameObjectActivePun(false);
                
                Transform t = transform;
                currentCommander = PlayerManager.ownedManager.CreateController(
                    rtsController,
                    t.position + spawnOffset,
                    Quaternion.identity);
                currentCommander.GetComponent<RtsController>().Setup(this);

                hash.Add("occupied", true);
            }

            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        public bool CanTrigger()
        {
            return canBeTriggered;
        }

        #endregion
    }
}