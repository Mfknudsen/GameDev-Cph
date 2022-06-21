#region Packages

using System;
using ExitGames.Client.Photon;
using GameDev.Multiplayer;
using GameDev.Weapons.Ammo;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

#endregion

namespace GameDev.Character
{
    #region Enums

    public enum HealthType
    {
        Marine,
        NonPlayer,
        Structure,
        Alien,
        Arc,
        Exosuit
    }

    #endregion

    public class Health : MonoBehaviourPunCallbacks
    {
        #region Values

        [SerializeField] private bool isPlayer, reactToDamage = true;
        [SerializeField] private PhotonView pv;
        [SerializeField] private HealthPreset healthPreset;
        [SerializeField] private HealthType healthType;
        [SerializeField] private Team team;

        public UnityEvent onDeathEvent = new UnityEvent();

        private PlayerManager playerManager;

        private float currentHealthPoints,
            currentArmorPoints;

        private readonly string receiveCurrentHealthString = "curHealth",
            receiveCurrentArmorString = "curArmor";

        #endregion

        #region Build In States

        public override void OnEnable()
        {
            if (!pv.IsMine) return;

            if (playerManager != null)
                playerManager.GetPlayerStats().onStatsChangeEvent.AddListener(OnPlayerStatChange);
        }

        public override void OnDisable()
        {
            if (!pv.IsMine) return;

            if (playerManager != null)
                playerManager.GetPlayerStats().onStatsChangeEvent.RemoveListener(OnPlayerStatChange);
        }

        private void Start()
        {
            currentHealthPoints = healthPreset.GetMaxHp();
            currentArmorPoints = healthPreset.GetMaxAp();

            if (!pv.IsMine) return;

            playerManager = PlayerManager.ownedManager;
        }

        #region Pun Callbacks

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (pv.Owner.Equals(targetPlayer) && !pv.IsMine)
            {
                if (changedProps.ContainsKey(receiveCurrentHealthString))
                    currentHealthPoints = (float)
                        changedProps[receiveCurrentHealthString];

                if (changedProps.ContainsKey(receiveCurrentArmorString))
                    currentArmorPoints = (float)
                        changedProps[receiveCurrentArmorString];
            }
        }

        #endregion

        #endregion

        #region Getters

        public float GetCurrentHp()
        {
            return currentHealthPoints;
        }

        public float GetCurrentAp()
        {
            return currentArmorPoints;
        }

        public HealthType GetHealthType()
        {
            return healthType;
        }

        public float GetMaxHp()
        {
            return healthPreset.GetMaxHp();
        }

        #endregion

        #region In

        public void StartTrigger()
        {
            reactToDamage = true;
        }

        public void ApplyDamage(float damage, DamageType damageType, SpecialDamageType specialDamageType,
            Team shooterTeam)
        {
            Debug.Log(shooterTeam.ToString());
            Debug.Log(team.ToString());
            
            if (shooterTeam == team) return;

            pv.RPC("RPCApplyDamage", RpcTarget.All, damage, damageType);
        }

        public void ApplyHealHp(float heal)
        {
            pv.RPC("RPCApplyHealHp", RpcTarget.All, heal);
        }

        public void ApplyHealAp(float heal)
        {
            pv.RPC("RPCApplyHealAp", RpcTarget.All, heal);
        }

        public void InstantKill()
        {
            onDeathEvent.Invoke();

            pv.RPC("RPCDeath", RpcTarget.Others);
        }

        #endregion

        #region Internal

        private void Die()
        {
            pv.RPC("RPCDeath", RpcTarget.Others);

            if (isPlayer)
                playerManager.Die();
            else
                onDeathEvent.Invoke();
        }

        private Vector2 CalculateDamage(float damage, DamageType damageType)
        {
            float damageArmor = damage / BasicArmorAbsorb(damageType);
            float damageHealth = 0;

            if (!(damageArmor > currentArmorPoints))
                return new Vector2(damageHealth, damageArmor);

            float diff = damageArmor - currentArmorPoints;
            damageArmor -= diff;
            damageHealth = diff * BasicArmorAbsorb(damageType);

            return new Vector2(damageHealth, damageArmor);
        }

        private static int BasicArmorAbsorb(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Normal => 2,
                DamageType.Light => 4,
                DamageType.Heavy => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null)
            };
        }

        private void OnPlayerStatChange()
        {
        }

        #region Pun RPC

        #region Owned

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCApplyDamage(float damage, DamageType damageType)
        {
            if (!pv.IsMine || !reactToDamage) return;

            //Health: x
            //Armor: y
            Vector2 damageTotal = CalculateDamage(damage, damageType);

            #region Apply Damage

            currentHealthPoints = Mathf.Clamp(
                currentHealthPoints - damageTotal.x,
                0,
                healthPreset.GetMaxHp());

            currentArmorPoints = Mathf.Clamp(
                currentArmorPoints - damageTotal.y,
                0,
                healthPreset.GetMaxAp());

            #endregion

            pv.RPC("RPCUpdateOthers", RpcTarget.Others, currentHealthPoints, currentArmorPoints);

            if (currentHealthPoints == 0)
                Die();
        }

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCApplyHealHp(float heal)
        {
            if (!pv.IsMine || !reactToDamage) return;

            currentHealthPoints = Mathf.Clamp(
                currentHealthPoints + heal,
                0,
                healthPreset.GetMaxHp());

            pv.RPC("RPCUpdateOthers", RpcTarget.Others, currentHealthPoints, currentArmorPoints);
        }

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCApplyHealAp(float heal)
        {
            if (!pv.IsMine) return;

            currentArmorPoints = Mathf.Clamp(
                currentArmorPoints + heal,
                0,
                healthPreset.GetMaxHp());

            pv.RPC("RPCUpdateOthers", RpcTarget.Others, currentHealthPoints, currentArmorPoints);
        }

        #endregion

        #region Sync

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCUpdateOthers(float curHP, float curAP)
        {
            currentArmorPoints = curAP;
            currentHealthPoints = curHP;
        }

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCDeath()
        {
            if (isPlayer)
                PlayerManager.GetManagerByPhotonOwner(pv.Owner).Die();
            else
                onDeathEvent.Invoke();
        }

        #endregion

        #endregion

        #endregion
    }
}