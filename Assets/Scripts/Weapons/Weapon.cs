#region Packages

using System;
using ExitGames.Client.Photon;
using GameDev.Input;
using GameDev.Multiplayer;
using GameDev.Weapons.Ammo;
using GameDev.Weapons.Triggers;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

#endregion

namespace GameDev.Weapons
{
    [Serializable]
    public abstract class Weapon : MonoBehaviourPunCallbacks
    {
        #region Values

        [SerializeField] protected PhotonView pv;
        [SerializeField] protected Trigger trigger;
        [SerializeField] protected Transform origin;
        [SerializeField] protected Ammunition ammunition;
        [SerializeField] protected int magMaxSize, magCurSize, ammoPerShot, weaponLevel;
        [SerializeField] protected Team team;
        
        protected bool shooting, reloading;

        #endregion

        #region Build In States

        public override void OnEnable()
        {
            if (!pv.IsMine)
                return;

            InputManager.instance.shootEvent.AddListener(OnShootUpdate);
            InputManager.instance.reloadEvent.AddListener(OnReloadUpdate);

            PlayerManager.ownedManager?.GetPlayerStats().onStatsChangeEvent.AddListener(OnShootUpdate);
        }

        public override void OnDisable()
        {
            if (!pv.IsMine)
                return;

            InputManager.instance.shootEvent.RemoveListener(OnShootUpdate);
            InputManager.instance.reloadEvent.RemoveListener(OnReloadUpdate);

            PlayerManager.ownedManager.GetPlayerStats().onStatsChangeEvent.RemoveListener(OnShootUpdate);
        }

        private void Start()
        {
            trigger = Instantiate(trigger);
        }

        protected virtual void Awake()
        {
            pv ??= GetComponent<PhotonView>();
        }

        #region Pun Callbacks

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (pv.IsMine || !pv.Owner.Equals(targetPlayer)) return;

            if (changedProps.ContainsKey("Shooting"))
                shooting = (bool)changedProps["Shooting"];

            if (changedProps.ContainsKey("Reloading"))
                reloading = (bool)changedProps["Reloading"];
        }

        #endregion

        #endregion

        #region Getters

        public int GetCurrentMagCount()
        {
            return magCurSize;
        }

        public int GetMaxMagCount()
        {
            return magMaxSize;
        }

        #endregion

        #region Internal

        protected abstract void Trigger();

        protected abstract void Reload();

        private void OnShootUpdate()
        {
            shooting = !shooting;

            if (shooting && !reloading && magCurSize > 0)
                Trigger();

            Hashtable hash = new Hashtable();
            hash.Add("Shooting", shooting);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        private void OnReloadUpdate()
        {
            Reload();
        }

        protected virtual void OnStatChange()
        {
            try
            {
                weaponLevel = (int)PlayerManager.ownedManager.GetPlayerStats()
                    .GetStatValueByKey(PlayerStat.WeaponLevel);
            }
            catch
            {
                weaponLevel = 1;
            }
        }

        #endregion
    }
}