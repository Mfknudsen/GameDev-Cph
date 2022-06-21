#region Packages

using System;
using System.Linq;
using GameDev.Character;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

#endregion

namespace GameDev.Multiplayer
{
    public class Objective : MonoBehaviourPunCallbacks
    {
        #region Values

        public static readonly int minCount = 6;

        [HideInInspector] public UnityEvent onEndEvent = new UnityEvent();
        [HideInInspector] public UnityEvent<float[]> updateUIEvent = new UnityEvent<float[]>();

        [SerializeField] private Objective nextObjective;
        [SerializeField] private Health[] objectivesHealth;
        [SerializeField] private PhotonView pv;
        [SerializeField] private bool isFirst;

        private bool started;

        #endregion

        #region Build In States

        private void Update()
        {
            if (!started || !PhotonNetwork.MasterClient.CustomProperties.ContainsKey("Start")) return;

            float[] toUpdate = objectivesHealth.Select(o => o.GetCurrentHp() / o.GetMaxHp()).ToArray();
            updateUIEvent.Invoke(toUpdate);

            if (!pv.IsMine) return;

            pv.RPC("RPCUpdate", RpcTarget.Others,
                objectivesHealth.Select(h => h.GetCurrentHp() / h.GetMaxHp()).ToArray());

            if (objectivesHealth.Select(h => h.GetCurrentHp()).Sum() <= 0)
            {
                if (nextObjective != null)
                    nextObjective.StartObjective();
                else
                    HostManager.instance.EndGame(Team.Alien);

                onEndEvent.Invoke();
                Destroy(gameObject);
            }
            else if (PhotonNetwork.MasterClient.CustomProperties.ContainsKey("Start"))
            {
                if (int.Parse((DateTime.Parse((string)PhotonNetwork.MasterClient.CustomProperties["Start"])
                        .AddMinutes(minCount) - DateTime.Now).ToString(@"mm")) <= 0)
                    HostManager.instance.EndGame(Team.Human);
            }
        }
    

    #endregion

        #region Getters

        public bool GetStarted()
        {
            return started;
        }

        public bool GetFirst()
        {
            return isFirst;
        }

        #endregion

        #region In

        public void StartObjective()
        {
            foreach (Health health in objectivesHealth)
                health.StartTrigger();

            started = true;
        }

        #endregion

        #region Internal

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCUpdate(float[] input)
        {
            updateUIEvent.Invoke(input);
        }

        #endregion
    }
}