#region Packages

using System.Collections;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Terrain.Doors
{
    public class BreakableDoor : MonoBehaviourPunCallbacks
    {
        #region Values

        [SerializeField] private Transform openPoint;
        [SerializeField] private float speed;

        private Transform visualTransform;

        #endregion

        #region In

        public void StartOpen()
        {
            visualTransform = transform.GetChild(0);

            StartCoroutine(Open());
        }

        #endregion

        #region Internal

        private IEnumerator Open()
        {
            Vector3 dir = (openPoint.position - visualTransform.position).normalized;
            
            while (Vector3.Distance(openPoint.position, visualTransform.position) > 0.5f)
            {
                visualTransform.position += dir * (speed * Time.deltaTime);

                yield return null;
            }

            PhotonNetwork.Destroy(gameObject);
        }

        #endregion
    }
}