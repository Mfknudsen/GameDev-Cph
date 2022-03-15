#region Packages

using UnityEngine;

#endregion

namespace GameDev.Multiplayer
{
    public class TeamSelector : MonoBehaviour
    {
        private void Awake()
        {
            Cursor.visible = true;
        }

        #region In

        public void SelectTeam(int set)
        {
            HostManager.instance.SetTeam(PlayerManager.ownedManager.GetPhotonView().Owner.NickName, (Team)set);

            Destroy(gameObject);
        }

        #endregion
    }
}