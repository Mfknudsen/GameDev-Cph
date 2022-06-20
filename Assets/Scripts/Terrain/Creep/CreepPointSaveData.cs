#region Packages

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace GameDev.Terrain.Creep
{
    [Serializable]
    public struct CreepPointSaveData
    {
        [SerializeField] public Vector3Int index;
        [SerializeField] public List<Vector3Int> connected;
        [SerializeField] public Vector3 normal, world;
    }
}