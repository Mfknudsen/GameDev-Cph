#region Packages

using System;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Buildings
{
    [Serializable]
    public abstract class Building : MonoBehaviourPunCallbacks
    {
        #region Values

        [SerializeField] protected PhotonView pv;
        [SerializeField] protected int cost;

        #endregion

        #region Build In States

        private void Start()
        {
            pv ??= GetComponent<PhotonView>();

            if (pv.InstantiationData != null && pv.InstantiationData.Length > 0 && (bool) pv.InstantiationData[0])
                OnInstantiatedStart();
            else
                OnLocalStart();
        }

        #endregion

        #region Getters

        public int GetCost()
        {
            return cost;
        }

        #endregion

        #region In

        public void Place()
        {
            Transform trans = transform;
            PhotonNetwork.Instantiate(gameObject.name, trans.position, trans.rotation, 0, new object[] {true});
            Destroy(gameObject);
        }

        public void Destroy()
        {
            PhotonNetwork.Destroy(gameObject);
        }

        #endregion

        #region Out

        public virtual bool CanBePlaced()
        {
            return true;
        }

        #endregion

        #region Internal

        protected virtual void OnInstantiatedStart()
        {
        }

        protected virtual void OnLocalStart()
        {
        }

        #endregion
    }
}