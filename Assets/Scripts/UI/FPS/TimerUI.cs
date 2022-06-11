#region Packages

using System;
using GameDev.Common;
using GameDev.Multiplayer;
using Photon.Pun;
using TMPro;
using UnityEngine;

#endregion

namespace GameDev.UI.FPS
{
    public class TimerUI : MonoBehaviour
    {
        #region Values

        [SerializeField] private TextMeshProUGUI textMeshProUGUI;

        private bool started;
        private DateTime startTime;

        #endregion

        #region Build In States

        private void Update()
        {
            if (started) return;

            if (!PhotonNetwork.MasterClient.CustomProperties.ContainsKey("Start")) return;

            startTime = DateTime.Parse((string)PhotonNetwork.MasterClient.CustomProperties["Start"])
                .AddMinutes(Objective.minCount);

            started = true;

            UpdateTimer();
        }

        #endregion

        #region Internal

        private void UpdateTimer()
        {
            textMeshProUGUI.text = (startTime - DateTime.Now).ToString(@"mm");

            new Timer(TimerType.Seconds, .1f).timerEvent.AddListener(UpdateTimer);
        }

        #endregion
    }
}