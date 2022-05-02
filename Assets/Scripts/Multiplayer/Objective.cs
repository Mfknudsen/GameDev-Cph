#region Packages

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

        public UnityEvent onEndEvent = new UnityEvent();
        [SerializeField] private Objective nextObjective;
        [SerializeField] private Health[] objectivesHealth;
        [SerializeField] private PhotonView pv;
        [SerializeField] private bool isFirst;

        private bool started;

        #endregion

        #region Build In States

        private void Start()
        {
            if (!pv.IsMine) return;

            onEndEvent.AddListener(nextObjective.StartObjective);

            if (!isFirst) return;

            StartObjective();
        }

        private void Update()
        {
            if (!started || !pv.IsMine) return;

            if (objectivesHealth.Select(h => h.GetCurrentHp()).Sum() <= 0)
            {
                onEndEvent.Invoke();
                Destroy(gameObject);
            }
            else
                pv.RPC("RPCUpdate", RpcTarget.Others, objectivesHealth.Select(h => h.GetCurrentHp()).ToArray());
        }

        #endregion

        #region In

        public void StartObjective()
        {
            foreach (Health health in objectivesHealth)
                health.StartTrigger();
        }

        #endregion

        #region Internal

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCUpdate(int[] input)
        {
            if (pv.IsMine) return;

            if (input.Sum() <= 0)
                Debug.Log("First Objective Destroyed");
        }

        #endregion
    }
}