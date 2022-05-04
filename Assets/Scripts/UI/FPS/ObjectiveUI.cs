#region Packages

using System.Linq;
using GameDev.Multiplayer;
using UnityEngine;

#endregion

namespace GameDev.UI.FPS
{
    public class ObjectiveUI : MonoBehaviour
    {
        #region Values

        [SerializeField] private Healthbar[] bars;

        private Objective current;

        #endregion

        #region Build In States

        private void Awake()
        {
            current = FindObjectsOfType<Objective>().First(o => o.GetFirst());
            current.updateUIEvent.AddListener(UpdateBars);
        }

        #endregion

        #region In

        public void UpdateCurrent(Objective obj)
        {
            current.updateUIEvent.RemoveListener(UpdateBars);
            current = obj;
            current.updateUIEvent.AddListener(UpdateBars);
        }

        #endregion

        #region Internal

        private void UpdateBars(float[] input)
        {
            for (int i = 0; i < bars.Length; i++)
                bars[i].Set(input[i]);
        }

        #endregion
    }
}