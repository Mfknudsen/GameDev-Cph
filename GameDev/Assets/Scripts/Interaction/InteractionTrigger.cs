#region Packages

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace GameDev.Interaction
{
    public class InteractionTrigger : MonoBehaviour
    {
        #region Values

        [SerializeField] private List<GameObject> gameObjectsToTrigger;

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

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.GetComponent<InteractionSelector>() is { } selector)
                selector.Add(this);
        }

        private void OnCollisionExit(Collision other)
        {
            if (other.gameObject.GetComponent<InteractionSelector>() is { } selector)
                selector.Add(this);
        }

        #endregion
    }
}