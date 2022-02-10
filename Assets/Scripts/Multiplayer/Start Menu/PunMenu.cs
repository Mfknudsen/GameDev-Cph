#region Packages

using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

#endregion

namespace GameDev.Multiplayer.Start_Menu
{
    public class PunMenu : MonoBehaviourPunCallbacks
    {
        #region Values

        [SerializeField] private TextMeshProUGUI messageDisplay;
        [SerializeField] private TMP_InputField displayNameInputField, serverNameInputField;

        [Space] [SerializeField] private GameObject serverDisplayPrefab;
        [SerializeField] private Transform listTransform;

        private ServerDisplay selectedDisplay;

        #endregion

        #region Build In States

        private void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        #endregion

        #region Setters

        public void SetSelectedDisplay(ServerDisplay set)
        {
            selectedDisplay = set;
        }

        #endregion

        #region In

        public void HostNew()
        {
            if (serverNameInputField.text == "")
            {
                messageDisplay.text = "New Server Must A Name";
                return;
            }

            PhotonNetwork.CreateRoom(
                serverNameInputField.text,
                new RoomOptions()
                {
                    MaxPlayers = 25,
                    BroadcastPropsChangeToAll = true,
                    IsVisible = true
                });
        }

        public void JoinRandom()
        {
            if (displayNameInputField.text == "")
            {
                messageDisplay.text = "You Must Have A Nickname";
                return;
            }

            PhotonNetwork.JoinRandomRoom();
        }

        public void JoinSelected()
        {
            if (selectedDisplay == null)
            {
                messageDisplay.text = "Please Select A Room From The List";
                return;
            }

            if (selectedDisplay.GetPlayerCount() == 24)
            {
                messageDisplay.text = "Server Is Full";
                return;
            }

            if (displayNameInputField.text == "")
            {
                messageDisplay.text = "Must Have A Nickname";
                return;
            }

            PhotonNetwork.JoinRoom(selectedDisplay.GetServerName());
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            PhotonNetwork.LocalPlayer.NickName = displayNameInputField.text;

            PhotonNetwork.LoadLevel("TestWorld");
        }

        #endregion

        #region Internal

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (RoomInfo roomInfo in roomList)
            {
                string serverName = roomInfo.Name;
                int playerCount = roomInfo.PlayerCount;
                
                Instantiate(serverDisplayPrefab, listTransform).GetComponent<ServerDisplay>()
                    .Setup(serverName, playerCount, this);
            }
        }

        #endregion
    }
}