#region Packages

using System;
using GameDev.Multiplayer;
using GameDev.RTS;
using GameDev.UI.RTS;
using GameDev.UI.RTS.Grid;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Buildings
{
    [Serializable]
    public abstract class Building : MonoBehaviourPunCallbacks, ISelectable
    {
        #region Values

        [SerializeField] protected PhotonView pv;
        [SerializeField] protected int cost;

        [SerializeField] protected Team team;

        private Selector currentSelector;

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

        public override void OnDisable()
        {
            base.OnDisable();

            if (currentSelector != null)
                currentSelector.RemoveSelectedFromList(this);
        }

        #endregion

        #region Getters

        public int GetCost()
        {
            return cost;
        }

        public Team GetTeam()
        {
            return team;
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

        public void OnSelect(Selector selector)
        {
            selector.AddSelectedToList(this);
        }

        public void OnFocus(RtsUI ui)
        {
            AddToActionMenu(ui.GetActionMenu());
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

        public void OnDeselect(Selector selector)
        {
            currentSelector = null;
            selector.RemoveSelectedFromList(this);
        }

        protected abstract void AddToActionMenu(GridMenu actionMenu);

        #endregion
    }
}