#region Packages

using System;
using System.Linq;
using ExitGames.Client.Photon;
using GameDev.Multiplayer;
using GameDev.Weapons.Ammo;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

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

        [SerializeField] private PhotonView pv;
        [SerializeField] private HealthPreset healthPreset;
        [SerializeField] private HealthType healthType;

        private PlayerManager playerManager;

        private float currentHealthPoints,
            currentArmorPoints;

        private readonly string receiveCurrentHealthString = "curHealth",
            receiveCurrentArmorString = "curArmor";

        #endregion

        #region Build In States

        private void Start()
        {
            currentHealthPoints = healthPreset.GetMaxHp();
            currentArmorPoints = healthPreset.GetMaxAp();

            if (!pv.IsMine) return;

            playerManager = FindObjectsOfType<PlayerManager>()
                .Where(o => o.gameObject.GetComponent<PhotonView>().Owner.Equals(pv.Owner)).First();
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

        #endregion

        #region In

        public void ApplyDamage(float damage, DamageType damageType, SpecialDamageType specialDamageType)
        {
            pv.RPC("RPCApplyDamage", RpcTarget.All, new object[] {damage, damageType, specialDamageType});
        }

        public void ApplyHealHp(float heal)
        {
            pv.RPC("RPCApplyHealHp", RpcTarget.All, heal);
        }

        public void ApplyHealAp(float heal)
        {
            pv.RPC("RPCApplyHealAp", RpcTarget.All, heal);
        }

        #endregion

        #region Internal

        private void Die()
        {
            playerManager.Die();
        }

        private Vector2 CalculateDamage(float damage, DamageType damageType, SpecialDamageType specialDamageType)
        {
            float damageArmor = damage / BasicArmorAbsorb(damageType);
            float damageHealth = 0;

            if (damageArmor > currentArmorPoints)
            {
                float diff = damageArmor - currentArmorPoints;
                damageArmor -= diff;
                damageHealth = diff * BasicArmorAbsorb(damageType);
            }

            return new Vector2(damageHealth, damageArmor);
        }

        private Vector2 DamageModifier(SpecialDamageType specialDamageType)
        {
            return Vector2.one;
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

        #region Pun RPC

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCApplyDamage(float damage, DamageType damageType, SpecialDamageType specialDamageType)
        {
            if (!pv.IsMine) return;

            //Health: x
            //Armor: y
            Vector2 damageTotal = CalculateDamage(damage, damageType, specialDamageType);

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

            #region Sync

            Hashtable hash = new Hashtable();
            hash.Add(
                receiveCurrentHealthString,
                currentHealthPoints);
            hash.Add(
                receiveCurrentArmorString,
                currentArmorPoints);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

            #endregion

            if (currentHealthPoints == 0)
                Die();
        }

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCApplyHealHp(float heal)
        {
            if (!pv.IsMine) return;

            currentHealthPoints = Mathf.Clamp(
                currentHealthPoints + heal,
                0,
                healthPreset.GetMaxHp());

            Hashtable hash = new Hashtable();
            hash.Add(receiveCurrentHealthString, currentHealthPoints);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
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

            Hashtable hash = new Hashtable();
            hash.Add(receiveCurrentArmorString, currentArmorPoints);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        #endregion

        #endregion
    }
}