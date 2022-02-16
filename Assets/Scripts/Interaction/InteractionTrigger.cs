#region Packages

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace GameDev.Interaction
{
    public class InteractionTrigger : MonoBehaviour
    {
        #region Values

        [SerializeField] private GameObject uiMessagePrefab;

        [SerializeField] private List<GameObject> gameObjectsToTrigger = new List<GameObject>();

        #endregion

        #region Build In States

        private void OnValidate()
        {
            foreach (GameObject obj in gameObjectsToTrigger)
            {
                if (obj.GetComponent<IInteract>() != null)
                    continue;

                gameObjectsToTrigger.Remove(obj);
                Debug.LogError($"GameObject: {obj.name} does not contain");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<InteractionSelector>() is { } selector)
                selector.Add(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetComponent<InteractionSelector>() is { } selector)
                selector.Remove(this);
        }

        #endregion

        #region Getters

        public GameObject GetUIPrefab()
        {
            return uiMessagePrefab;
        }

        #endregion

        #region In

        public void Trigger()
        {
            foreach (GameObject obj in gameObjectsToTrigger)
            {
                IInteract[] interacts = obj.GetComponents<MonoBehaviour>().OfType<IInteract>().Select(o => o).ToArray();
                foreach (IInteract interact in interacts)
                    interact.TriggerInteraction();
            }
        }

        #endregion
    }
}