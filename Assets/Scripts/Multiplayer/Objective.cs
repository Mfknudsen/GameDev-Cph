#region Packages

using System.Linq;
using GameDev.Character;
using GameDev.UI.FPS;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

#endregion

namespace GameDev.Multiplayer
{
    public class Objective : MonoBehaviourPunCallbacks
    {
        #region Values

        [HideInInspector] public UnityEvent onEndEvent = new UnityEvent();
        [HideInInspector] public UnityEvent<float[]> updateUIEvent = new UnityEvent<float[]>();

        [SerializeField] private Objective nextObjective;
        [SerializeField] private Health[] objectivesHealth;
        [SerializeField] private PhotonView pv;
        [SerializeField] private bool isFirst;

        private bool started;

        #endregion

        #region Build In States

        private void Start()
        {
            onEndEvent.AddListener(() =>
            {
                nextObjective.StartObjective();
                started = false;
                foreach (ObjectiveUI objectiveUI in FindObjectsOfType<ObjectiveUI>())
                    objectiveUI.UpdateCurrent(nextObjective);
            });

            if (!isFirst || !pv.IsMine) return;

            StartObjective();
        }

        private void Update()
        {
            if (!started) return;

            float[] toUpdate = objectivesHealth.Select(o => o.GetCurrentHp() / o.GetMaxHp()).ToArray();
            updateUIEvent.Invoke(toUpdate);

            if (!pv.IsMine) return;

            if (objectivesHealth.Select(h => h.GetCurrentHp()).Sum() <= 0)
            {
                onEndEvent.Invoke();
                Destroy(gameObject);
            }
            else
                pv.RPC("RPCUpdate", RpcTarget.Others, objectivesHealth.Select(h => h.GetCurrentHp()).ToArray());
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