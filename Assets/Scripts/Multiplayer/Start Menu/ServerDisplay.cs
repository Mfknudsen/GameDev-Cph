#region Packages

using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace GameDev.Multiplayer.Start_Menu
{
    public class ServerDisplay : MonoBehaviour
    {
        #region Values

        [SerializeField] private TextMeshProUGUI textField;

        private string serverName;
        private int playerCount;

        private PunMenu punMenu;

        #endregion

        #region Getters

        public string GetServerName()
        {
            return serverName;
        }

        public int GetPlayerCount()
        {
            return playerCount;
        }

        #endregion

        #region In

        public void Setup(string sName, int count, PunMenu menu)
        {
            serverName = sName;
            playerCount = count;
            punMenu = menu;

            textField.text = sName + "\nPlayers: " + (count - 1);
        }

        public void OnClick()
        {
            punMenu.SetSelectedDisplay(this);
        }

        public void Highlight(bool set)
        {
            GetComponent<Image>().color = set ? Color.green : Color.black;
        }

        #endregion
    }
}