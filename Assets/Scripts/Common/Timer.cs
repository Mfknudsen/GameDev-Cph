#region Packages

using UnityEngine;
using UnityEngine.Events;

#endregion

namespace GameDev.Common
{
    public class Timer
    {
        #region Values

        private readonly float duration;
        private float current;
        public readonly UnityEvent timerEvent;
        private bool done;

        #endregion

        #region Build In States

        public Timer(float duration)
        {
            this.duration = duration;

            current = 0;

            timerEvent = new UnityEvent();

            TimerUpdater.instance.timers.Add(this);
        }

        #endregion

        #region Getters

        public bool GetDone()
        {
            return done;
        }

        #endregion

        #region In

        public void Update()
        {
            if (done) return;

            current += Time.deltaTime;

            if (current < duration)
                return;

            done = true;

            timerEvent.Invoke();
            TimerUpdater.instance.timers.Remove(this);
        }

        #endregion

        #region Out

        public int GetPercentDone()
        {
            return (int)(current / duration * 100);
        }

        #endregion
    }
}