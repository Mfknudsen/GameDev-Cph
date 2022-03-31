#region Packages

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace GameDev.Terrain
{
    [Serializable]
    public struct CreepPointSaveData
    {
        [SerializeField] public Vector3Int index;
        [SerializeField] public List<Vector3Int> connected;
    }
}