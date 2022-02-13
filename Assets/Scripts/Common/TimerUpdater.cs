#region Packages

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace GameDev.Common
{
    public class TimerUpdater : MonoBehaviour
    {
        public static TimerUpdater instance;
        public List<Timer> timers = new List<Timer>();

        private void Start()
        {
            if (instance != null)
                Destroy(gameObject);

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            Timer[] toUpdate = timers.Where(t => t != null).ToArray();
            
            foreach (Timer timer in toUpdate.Where(t => t != null))
                timer.Update();
        }
    }
}