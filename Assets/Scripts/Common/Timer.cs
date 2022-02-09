#region Packages

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#endregion

namespace GameDev
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
        }

        #endregion

        #region Getters

        public bool IsDone()
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