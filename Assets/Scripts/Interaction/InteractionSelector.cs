#region Packages

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace GameDev.Interaction
{
    public class InteractionSelector : MonoBehaviour
    {
        #region Values

        private List<InteractionTrigger> inRange = new List<InteractionTrigger>();

        #endregion

        #region In

        public void Add(InteractionTrigger interact)
        {
            if (!inRange.Contains(interact))
                inRange.Add(interact);
        }

        public void Remove(InteractionTrigger interact)
        {
            if (inRange.Contains(interact))
                inRange.Remove(interact);
        }

        #endregion
    }
}