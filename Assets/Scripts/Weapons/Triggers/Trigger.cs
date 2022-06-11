#region Packages

using GameDev.Common;
using UnityEngine;

#endregion

namespace GameDev.Weapons.Triggers
{
    [CreateAssetMenu(menuName = "GameDev/Trigger")]
    public class Trigger : ScriptableObject
    {
        #region Values

        [SerializeField] private float timerDuration;

        private bool canFire = true;
        private Timer inBetweenDelay;

        #endregion

        #region Getters

        public bool GetCanFire()
        {
            return canFire;
        }

        #endregion

        #region In

        public void Pull()
        {
            canFire = false;

            inBetweenDelay = new Timer(TimerType.Seconds, timerDuration);
            inBetweenDelay.timerEvent.AddListener(() =>
            {
                canFire = true;
                inBetweenDelay = null;
            });
        }

        #endregion
    }
}