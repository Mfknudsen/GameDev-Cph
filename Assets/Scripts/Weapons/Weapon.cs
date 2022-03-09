#region Packages

using System;
using ExitGames.Client.Photon;
using GameDev.Input;
using GameDev.Weapons.Triggers;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

#endregion

namespace GameDev.Weapons
{
    [Serializable]
    public abstract class Weapon : MonoBehaviourPunCallbacks, IWeapon
    {
        #region Values

        [SerializeField] protected PhotonView pv;
        [SerializeField] protected Trigger trigger;
        [SerializeField] protected Transform origin;
        
        protected bool shooting, reloading;

        #endregion

        #region Build In States

        private void Start()
        {
            trigger = Instantiate(trigger);
        }

        protected virtual void Awake()
        {
            pv ??= GetComponent<PhotonView>();

            if (pv.IsMine)
            {
                InputManager.Instance.shootEvent.AddListener(OnShootUpdate);
                InputManager.Instance.reloadEvent.AddListener(OnReloadUpdate);
            }
        }

        protected virtual void Update()
        {
            if (reloading)
            {
                Reload();
                return;
            }
            
            if (shooting && trigger.GetCanFire())
            {
                trigger.Pull();
                Trigger();
            }
        }

        #region Pun Callbacks

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (pv.IsMine || !pv.Owner.Equals(targetPlayer)) return;

            if (changedProps.ContainsKey("Shooting"))
                shooting = (bool) changedProps["Shooting"];

            if (changedProps.ContainsKey("Reloading"))
                reloading = (bool) changedProps["Reloading"];
        }

        #endregion

        #endregion

        #region Internal

        public abstract void Trigger();

        public abstract void Reload();

        private void OnShootUpdate()
        {
            shooting = !shooting;

            Hashtable hash = new Hashtable();
            hash.Add("Shooting", shooting);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        private void OnReloadUpdate()
        {
            reloading = !reloading;

            Hashtable hash = new Hashtable();
            hash.Add("Reloading", reloading);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        #endregion
    }
}