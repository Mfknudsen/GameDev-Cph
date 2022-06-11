#region Packages

using UnityEngine;
using UnityEngine.Events;

#endregion

namespace GameDev.Common
{
    #region Enums

    public enum TimerType
    {
        Frames,
        Seconds
    };

    #endregion

    public class Timer
    {
        #region Values

        public readonly UnityEvent timerEvent;
        private readonly TimerType timerType;
        private readonly float duration;
        private readonly bool repeat;
        private float current;
        private bool done;

        #endregion

        #region Build In States

        public Timer(TimerType timerType, float duration, bool? repeat = false)
        {
            this.duration = duration;
            this.repeat = repeat.HasValue ? repeat.Value : false;
            this.timerType = timerType;

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

            current += timerType == TimerType.Seconds ? Time.deltaTime : 1;

            if (current < duration)
                return;

            done = true;

            timerEvent.Invoke();

            if (!repeat)
            {
                TimerUpdater.instance.timers.Remove(this);
                return;
            }

            current = 0;
            done = false;
        }

        public void ForceEnd()
        {
            done = true;
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