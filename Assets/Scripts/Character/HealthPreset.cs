#region Packages

using UnityEngine;

#endregion

namespace GameDev.Character
{
    [CreateAssetMenu(menuName = "GameDev/Health Preset", fileName = "New Health Preset")]
    public sealed class HealthPreset : ScriptableObject
    {
        #region Values

        [SerializeField] private float maxHp, maxAp;

        #endregion

        #region Getters

        public float GetMaxHp()
        {
            return maxHp;
        }

        public float GetMaxAp()
        {
            return maxAp;
        }

        #endregion
    }
}