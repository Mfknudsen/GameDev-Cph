#region Packages

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace GameDev.Common
{
    public static class CommonGameObject
    {
        public static T[] GetAllComponentsByRoot<T>(GameObject gameObject) where T : Component
        {
            List<T> result = new List<T>();

            if (gameObject.GetComponent<T>() is T component)
                result.Add(component);

            foreach (Transform child in gameObject.transform)
                result.AddRange(GetAllComponentsByRoot<T>(child.gameObject));

            return result.Where(i => i != null).ToArray();
        }
        
        
        public static T[] GetAllMonoBehavioursByRoot<T>(GameObject gameObject) where T : MonoBehaviour
        {
            List<T> result = new List<T>();

            if (gameObject.GetComponent<T>() is T monoBehaviour)
                result.Add(monoBehaviour);

            foreach (Transform child in gameObject.transform)
                result.AddRange(GetAllComponentsByRoot<T>(child.gameObject));

            return result.Where(i => i != null).ToArray();
        }
    }
}