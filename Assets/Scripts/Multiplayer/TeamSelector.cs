using UnityEngine;

namespace GameDev.Multiplayer
{
    public class TeamSelector : MonoBehaviour
    {
        #region In

        public void SelectTeam(int set)
        {
            HostManager.instance.SetTeam(PlayerManager.ownedManager.GetPhotonView().Owner.UserId, (Team) set);
        }

        #endregion
    }
}