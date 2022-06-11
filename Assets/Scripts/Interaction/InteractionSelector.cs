#region Packages

using System.Collections.Generic;
using GameDev.Common;
using GameDev.Input;
using UnityEngine;

#endregion

namespace GameDev.Interaction
{
    public class InteractionSelector : MonoBehaviour
    {
        #region Values

        private List<InteractionTrigger> inRange = new List<InteractionTrigger>();
        private GameObject closestGameObject, currentUI;
        private InteractionTrigger closestTrigger;

        private bool interacting;

        #endregion

        #region Build In States

        private void Awake()
        {
            new Timer(TimerType.Seconds, 1f).timerEvent.AddListener(() => UpdateClosest());
        }

        private void OnEnable()
        {
            InputManager.instance.interactEvent.AddListener(OnInteractUpdate);
        }

        private void OnDisable()
        {
            InputManager.instance.interactEvent.RemoveListener(OnInteractUpdate);
        }

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

        #region Internal

        private void UpdateClosest()
        {
            GameObject previous = closestGameObject;

            if (inRange.Count > 0)
            {
                float currentDistance = closestGameObject == null
                    ? 100
                    : Vector3.Distance(transform.position, closestTrigger.transform.position);
                foreach (InteractionTrigger trigger in inRange)
                {
                    GameObject obj = trigger.gameObject;

                    float newDistance = Vector3.Distance(transform.position, obj.transform.position);
                    if (closestGameObject == null)
                    {
                        closestGameObject = obj;
                        closestTrigger = trigger;
                        currentDistance = newDistance;
                        continue;
                    }

                    if (newDistance < currentDistance)
                    {
                        currentDistance = newDistance;
                        closestGameObject = obj;
                        closestTrigger = trigger;
                    }
                }
            }
            else
            {
                closestGameObject = null;
                closestTrigger = null;
            }

            if (previous != closestGameObject)
            {
                Destroy(currentUI);

                if (closestGameObject != null && closestTrigger != null)
                    currentUI = Instantiate(closestTrigger.GetUIPrefab(), GameObject.Find("Canvas").transform);
            }

            new Timer(TimerType.Seconds, 0.5f).timerEvent.AddListener(() => UpdateClosest());
        }

        private void OnInteractUpdate()
        {
            interacting = !interacting;

            if (interacting && closestTrigger != null)
            {
                closestTrigger.Trigger();

                if (currentUI != null)
                    Destroy(currentUI);
            }
        }

        #endregion
    }
}