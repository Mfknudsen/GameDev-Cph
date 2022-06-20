#region Packages

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace GameDev.Terrain.Creep
{
    [Serializable]
    [CreateAssetMenu(menuName = "Terrain/Manager Data")]
    public class CreepManagerSaveData : ScriptableObject
    {
        [SerializeField] public List<CreepPointSaveData> pointSaveData = new List<CreepPointSaveData>();
    }
}