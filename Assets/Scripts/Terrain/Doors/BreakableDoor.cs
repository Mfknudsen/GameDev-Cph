#region Packages

using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Terrain.Doors
{
    public class BreakableDoor : MonoBehaviourPunCallbacks
    {
        #region Values
        
        [SerializeField] private Transform openPoint, visualTransform;
        [SerializeField] private float speed;

        private Vector3 startPoint;

        #endregion

        #region Build In States

        private void Start()
        {
            startPoint = visualTransform.position;

            if (PhotonNetwork.MasterClient.CustomProperties.ContainsKey("Start"))
            {
                // ReSharper disable once Unity.InefficientPropertyAccess
                Vector3 dir = (openPoint.position - visualTransform.position).normalized;

                float seconds = DateTime
                    .Parse((string)PhotonNetwork.MasterClient.CustomProperties["Start"])
                    .Subtract(DateTime.Now).Seconds;
                seconds = Mathf.Abs(seconds);

                // ReSharper disable once Unity.InefficientPropertyAccess
                visualTransform.position += dir * (speed * seconds);

                if (Vector3.Distance(openPoint.position, startPoint) <
                    Vector3.Distance(visualTransform.position, startPoint))
                    Destroy(gameObject);
                else
                    StartCoroutine(Open());
            }
        }

        #endregion

        #region In

        public void StartOpen()
        {
            StartCoroutine(Open());
        }

        #endregion

        #region Internal

        private IEnumerator Open()
        {
            Vector3 dir = (openPoint.position - visualTransform.position).normalized;

            while (true)
            {
                visualTransform.position += dir * (speed * Time.deltaTime);

                if (Vector3.Distance(openPoint.position, startPoint) <
                    Vector3.Distance(visualTransform.position, startPoint))
                {
                    Destroy(gameObject);

                    yield break;
                }
                
                yield return null;
            }
        }

        #endregion
    }
}