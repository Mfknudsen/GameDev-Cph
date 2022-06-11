#region Packages

using System.Collections.Generic;
using System.Linq;
using GameDev.Input;
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

        private readonly List<RoomInfo> activeServers = new List<RoomInfo>();

        private string sceneToLoadOnJoin;

        #endregion

        #region Build In States

        private void Start()
        {
            if (Camera.main != null)
                DontDestroyOnLoad(Camera.main.gameObject);

            InputManager.instance = new InputManager();

            Application.targetFrameRate = 60;

            PhotonNetwork.ConnectUsingSettings();
        }

        #region Pun Callbacks

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedRoom()
        {
            PhotonNetwork.LocalPlayer.NickName = displayNameInputField.text;
            sceneToLoadOnJoin = "MainMap";
            PhotonNetwork.LoadLevel(sceneToLoadOnJoin);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            messageDisplay.text = message;
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            bool selectedStillExits = false;
            string selectedName = selectedDisplay ? selectedDisplay.GetServerName() : "";
            selectedDisplay = null;

            //Destroy Current Visual List Content 
            foreach (Transform t in listTransform)
                Destroy(t.gameObject);

            //Check The Updated Info
            foreach (RoomInfo roomInfo in roomList)
            {
                if (roomInfo.RemovedFromList)
                    activeServers.Remove(activeServers?.First(s =>
                        s.Name.Equals(roomInfo.Name)));
                else if (!activeServers.Any(s =>
                             s.Name.Equals(roomInfo.Name)))
                    activeServers.Add(roomInfo);
            }

            //Setup Visual List Content
            foreach (RoomInfo activeServer in activeServers)
            {
                ServerDisplay display = Instantiate(serverDisplayPrefab, listTransform).GetComponent<ServerDisplay>();
                display.Setup(activeServer.Name, activeServer.PlayerCount, this);

                if (selectedStillExits || selectedName.Equals("")) continue;

                SetSelectedDisplay(display);
                selectedStillExits = true;
            }
        }

        #endregion

        #endregion

        #region Setters

        public void SetSelectedDisplay(ServerDisplay set)
        {
            if (selectedDisplay != null)
                selectedDisplay.Highlight(false);

            selectedDisplay = set;

            selectedDisplay.Highlight(true);
        }

        #endregion

        #region In

        #region Buttons

        public void HostNew()
        {
            if (displayNameInputField.text == "")
            {
                messageDisplay.text = "You Must Have A Nickname";
                return;
            }

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
            if (displayNameInputField.text == "")
            {
                messageDisplay.text = "You Must Have A Nickname";
                return;
            }

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

            PhotonNetwork.JoinRoom(selectedDisplay.GetServerName());
        }

        public void TestWorld()
        {
            sceneToLoadOnJoin = "TestWorld";

            PhotonNetwork.CreateRoom(
                serverNameInputField.text,
                new RoomOptions()
                {
                    MaxPlayers = 25,
                    BroadcastPropsChangeToAll = true,
                    IsVisible = true
                });
        }

        #endregion

        #endregion
    }
}