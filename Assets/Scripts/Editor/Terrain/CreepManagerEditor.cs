#region Packages

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameDev.Common;
using GameDev.Editor.Utilities;
using GameDev.Terrain;
using UnityEditor;
using UnityEngine;

#endregion

namespace GameDev.Editor.Terrain
{
    [CustomEditor(typeof(CreepManager))]
    public class CreepManagerEditor : UnityEditor.Editor
    {
        #region Values

        private bool generating;

        private int percent;

        #endregion

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!generating)
            {
                if (EditorGUILayout.Toggle(false))
                {
                    generating = true;
                    CreepManager manager = (CreepManager) target;
                    EditorCoroutines.Execute(GeneratePoints(manager, manager.GetPosition(), manager.GetSize(),
                        manager.GetPointsPer()));
                }
            }
            else
            {
                EditorGUILayout.LabelField(percent + "");
            }
        }

        private IEnumerator GeneratePoints(CreepManager manager, Vector3 origin, Vector3Int fieldSize,
            int pointsPerAxis)
        {
            CreepPoint[,,] creepPoints = new CreepPoint[fieldSize.x * pointsPerAxis, fieldSize.y * pointsPerAxis,
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
                            origin + new Vector3(x, y, z) / pointsPerAxis,
                            manager,
                            Mathf.PerlinNoise((x + y) * 0.9f, (z + y) * 0.9f)
                        );

                        if ((x + y + z) % 500 == 0)
                            yield return null;
                    }
                }
            }

            #endregion

            #region Point is surface

            int i = 0;
            foreach (CreepPoint creepPoint in creepPoints)
            {
                Collider col = CommonPhysic.GetNearestSurfaceBySphere(
                    creepPoint.worldPosition,
                    1.25f / pointsPerAxis,
                    manager.GetBuildMask()
                );

                creepPoint.active = col != null;

                if (col != null)
                {
                    Vector3 closestPoint = col.ClosestPoint(creepPoint.worldPosition);

                    creepPoint.normal = (creepPoint.worldPosition - closestPoint).normalized;
                    creepPoint.worldPosition = closestPoint + creepPoint.normal * 0.03f;
                }

                i++;
                if (i % 1000 == 0)
                    yield return null;
            }

            #endregion

            #region Usable

            List<Vector3Int> checkedPoints = new List<Vector3Int>(),
                toCheck = new List<Vector3Int>();

            toCheck.Add(CommonVariable.MultiDimensionalToList(creepPoints)
                .Where(cp => cp != null && cp.active)
                .OrderBy(cp => Vector3.Distance(cp.worldPosition, manager.GetClosestPointPosition())).First().index);

            int totalCount = CommonVariable.MultiDimensionalToList(creepPoints)
                .Where(cp => cp != null && cp.active)
                .OrderBy(cp => Vector3.Distance(cp.worldPosition, manager.GetClosestPointPosition()))
                .Count();
            int j = 0;
            while (toCheck.Count > 0)
            {
                CreepPoint point = creepPoints[toCheck[0].x, toCheck[0].y, toCheck[0].z];

                checkedPoints.Add(toCheck[0]);
                toCheck.RemoveAt(0);

                if (point == null || !point.active)
                    continue;

                foreach (Vector3Int neighborIndex in point.GetConnectedNeighbors())
                {
                    CreepPoint neighbor = creepPoints[neighborIndex.x, neighborIndex.y, neighborIndex.z];

                    if (neighbor == null || !neighbor.active)
                        continue;

                    Vector3 pPos = point.worldPosition + point.normal * 0.05f,
                        nPos = neighbor.worldPosition + neighbor.normal * 0.05f;
                    Vector3 dir = nPos - pPos;

                    if (!Physics.Raycast(neighbor.worldPosition, -dir.normalized, dir.magnitude, manager.GetBlockMask(),
                            QueryTriggerInteraction.Ignore) &&
                        !Physics.Raycast(point.worldPosition, dir.normalized, dir.magnitude, manager.GetBlockMask(),
                            QueryTriggerInteraction.Ignore))
                    {
                        if (!checkedPoints.Contains(neighbor.index) && !toCheck.Contains(neighbor.index))
                            toCheck.Add(neighbor.index);

                        point.AddConnected(neighbor.index);
                        neighbor.AddConnected(point.index);
                    }
                }

                j++;
                percent = (int) (j / (float) totalCount * 100);

                yield return null;
            }

            foreach (CreepPoint creepPoint in CommonVariable.MultiDimensionalToList(creepPoints)
                         .Where(cp => cp != null && cp.active))
                creepPoint.active = creepPoint.GetConnectedNeighbors().Length > 0;

            #endregion

            int count = 0;

            totalCount = CommonVariable.MultiDimensionalToList(creepPoints).Where(cp => cp != null && cp.active)
                .Count();

            CreepManagerSaveData managerSaveData = manager.GetSaveDataHolder();
            managerSaveData.pointSaveData = new List<CreepPointSaveData>();
            foreach (CreepPoint creepPoint in CommonVariable.MultiDimensionalToList(creepPoints)
                         .Where(cp => cp != null && cp.active))
            {
                CreepPointSaveData pointSaveData = new CreepPointSaveData()
                {
                    index = creepPoint.index,
                    connected = creepPoint.GetConnectedNeighbors().ToList()
                };
                managerSaveData.pointSaveData.Add(pointSaveData);

                count++;

                if (count % 50 == 0)
                {
                    EditorUtility.SetDirty(manager);
                    yield return 5;
                }

                percent = (int) (count / (float) totalCount * 100);
            }

            EditorUtility.SetDirty(manager);

            generating = false;
        }
    }
}