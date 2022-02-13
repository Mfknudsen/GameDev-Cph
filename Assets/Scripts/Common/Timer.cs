#region Packages

using UnityEngine;
using UnityEngine.Events;

#endregion

namespace GameDev.Common
{
    public class Timer
    {
        #region Values

        private float duration, current;
        public UnityEvent timerEvent;
        private bool done;

        #endregion

        #region Build In States

        public Timer(float duration)
        {
            this.duration = duration;

            current = 0;

            timerEvent = new UnityEvent();
            timerEvent.AddListener(() => TimerUpdater.instance.timers.Remove(this));

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
        }

        #endregion
    }
}