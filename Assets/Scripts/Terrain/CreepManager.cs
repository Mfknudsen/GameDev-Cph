#region Packages

using System.Collections.Generic;
using System.Linq;
using GameDev.Common;
using UnityEngine;

#endregion

namespace GameDev.Terrain
{
    public class CreepManager : MonoBehaviour
    {
        #region Values

        [SerializeField] private Vector3Int fieldSize = Vector3Int.one;
        [SerializeField] private int pointsPerAxis = 1;
        [SerializeField] private LayerMask mask;
        [SerializeField] private Transform checkFromPoint;
        [Range(0f, 1f)] [SerializeField] private float spreadPercentagePerSecond;
        [SerializeField] private Vector2 randomSpread = Vector2.zero;

        private CreepPoint[,,] creepPoints;

        private List<CreepPoint> toUpdateSource = new List<CreepPoint>();

        #endregion

        #region Build In States

        private void Start()
        {
            GenerateMesh();

            CreepPoint point = null;
            float dist = 10;
            foreach (CreepPoint creepPoint in creepPoints)
            {
                float newDist = Vector3.Distance(creepPoint.worldPosition, checkFromPoint.position);
                if (newDist < dist)
                {
                    dist = newDist;
                    point = creepPoint;
                }
            }

            if (point != null)
            {
                point.spread = 1;
                toUpdateSource.Add(point);
            }
        }

        private void Update()
        {
            UpdatePoints();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + (Vector3) fieldSize / 2, fieldSize);

            if (creepPoints == null)
                return;

            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (!creepPoint.active)
                    continue;

                Color c = Color.red;
                c.a = creepPoint.spread;
                Gizmos.color = c;
                // ReSharper disable once PossibleLossOfFraction
                Gizmos.DrawWireSphere(creepPoint.worldPosition, 0.5f / pointsPerAxis);
            }
        }

        #endregion

        #region Internal

        private void GenerateMesh()
        {
            Transform origin = transform;
            creepPoints = new CreepPoint[fieldSize.x * pointsPerAxis, fieldSize.y * pointsPerAxis,
                fieldSize.z * pointsPerAxis];

            #region Setup Points

            for (int x = 0; x < fieldSize.x * pointsPerAxis; x++)
            {
                for (int y = 0; y < fieldSize.y * pointsPerAxis; y++)
                {
                    for (int z = 0; z < fieldSize.z * pointsPerAxis; z++)
                    {
                        creepPoints[x, y, z] = new CreepPoint(
                            new Vector3Int(x, y, z),
                            origin.position + new Vector3(x, y, z) / pointsPerAxis
                        );
                    }
                }
            }

            #endregion

            #region Set neighbors

            foreach (CreepPoint creepPoint in creepPoints)
            {
                Vector3Int index = creepPoint.index;
                List<CreepPoint> neighborsToSet = new List<CreepPoint>();

                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        for (int z = -1; z < 2; z++)
                        {
                            Vector3Int tryIndex = index + new Vector3Int(x, y, z);
                            if (tryIndex.x < 0 || tryIndex.x >= fieldSize.x * pointsPerAxis ||
                                tryIndex.y < 0 || tryIndex.y >= fieldSize.y * pointsPerAxis ||
                                tryIndex.z < 0 || tryIndex.z >= fieldSize.z * pointsPerAxis)
                                continue;

                            neighborsToSet.Add(creepPoints[
                                tryIndex.x,
                                tryIndex.y,
                                tryIndex.z]);
                        }
                    }
                }

                creepPoint.SetNeighbors(neighborsToSet.ToArray());
            }

            #endregion

            #region Point is surface

            foreach (CreepPoint creepPoint in creepPoints)
            {
                Collider col = CommonPhysic.GetNearestSurfaceBySphere(
                    creepPoint.worldPosition,
                    1f / pointsPerAxis,
                    mask
                );

                creepPoint.active = col != null;
            }

            #endregion
        }

        private void UpdatePoints()
        {
            List<CreepPoint> toUpdate = new List<CreepPoint>(),
                toRemove = new List<CreepPoint>(),
                toAdd = new List<CreepPoint>();

            foreach (CreepPoint creepPoint in toUpdateSource)
            {
                CreepPoint[] spreadToNeighbors = creepPoint.GetNeighbors()
                    .Where(cp =>
                        cp.active &&
                        cp.spread < 1 &&
                        !toUpdate.Contains(cp))
                    .ToArray();

                if (spreadToNeighbors.Length == 0)
                {
                    toRemove.Add(creepPoint);
                    continue;
                }

                toUpdate.AddRange(spreadToNeighbors);
            }

            foreach (CreepPoint neighbor in toUpdate)
            {
                neighbor.spread =
                    Mathf.Clamp(
                        neighbor.spread + spreadPercentagePerSecond * Random.Range(randomSpread.x, randomSpread.y) *
                        Time.deltaTime,
                        0,
                        1);

                if (neighbor.spread > 0.5f)
                    toAdd.Add(neighbor);
            }

            toUpdateSource.AddRange(toAdd);
            toRemove.ForEach(e => toUpdateSource.Remove(e));
        }

        #endregion
    }
}