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

        private string currentPart = "";

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
                Rect r = EditorGUILayout.BeginVertical();
                GUILayout.Space(5);
                EditorGUI.ProgressBar(r, (float) percent / 100, currentPart);
                GUILayout.Space(20);
                EditorGUILayout.EndVertical();
            }
        }

        private IEnumerator GeneratePoints(CreepManager manager, Vector3 origin, Vector3Int fieldSize,
            int pointsPerAxis)
        {
            #region Setup Points

            CreepPoint[,,] creepPoints = new CreepPoint[fieldSize.x * pointsPerAxis, fieldSize.y * pointsPerAxis,
                fieldSize.z * pointsPerAxis];
            Dictionary<Vector3Int, Vector3Int[]> neighbors = new Dictionary<Vector3Int, Vector3Int[]>();
            int i = 0, totalCount;

            currentPart = "Setup";
            totalCount = creepPoints.Length;

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
                            0
                        );

                        i++;
                        if (i % 500 == 0)
                            yield return null;
                    }
                }
            }

            #endregion

            #region Point is surface

            currentPart = "Surface";
            i = 0;
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
                    creepPoint.worldPosition = closestPoint + creepPoint.normal * 0.5f;
                }

                i++;
                percent = (int) (i / (float) totalCount * 100);

                if (i % 500 == 0)
                    yield return null;
            }

            #endregion

            #region Neighbors

            currentPart = "Neighbors";

            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (creepPoint == null || !creepPoint.active)
                    continue;

                List<Vector3Int> toAdd = new List<Vector3Int>();
                for (int xAdd = -1; xAdd < 2; xAdd++)
                {
                    for (int yAdd = -1; yAdd < 2; yAdd++)
                    {
                        for (int zAdd = -1; zAdd < 2; zAdd++)
                        {
                            Vector3Int index = creepPoint.index + new Vector3Int(xAdd, yAdd, zAdd);

                            if (index == creepPoint.index ||
                                index.x < 0 || index.x >= creepPoints.GetLength(0) ||
                                index.y < 0 || index.y >= creepPoints.GetLength(1) ||
                                index.z < 0 || index.z >= creepPoints.GetLength(2))
                                continue;

                            toAdd.Add(index);
                        }
                    }
                }

                neighbors.Add(creepPoint.index, toAdd.ToArray());

                i++;
                percent = (int) (i / (float) totalCount * 100);

                if (i % 500 == 0)
                    yield return null;
            }

            #endregion

            #region Usable

            currentPart = "Usable";

            i = 0;
            List<Vector3Int> checkedPoints = new List<Vector3Int>(),
                toCheck = new List<Vector3Int>();

            toCheck.Add(CommonVariable.MultiDimensionalToList(creepPoints)
                .Where(cp => cp != null && cp.active)
                .OrderBy(cp => Vector3.Distance(cp.worldPosition, manager.GetClosestPointPosition())).First().index);

            totalCount = CommonVariable.MultiDimensionalToList(creepPoints)
                .Where(cp => cp != null && cp.active)
                .OrderBy(cp => Vector3.Distance(cp.worldPosition, manager.GetClosestPointPosition()))
                .Count();

            while (toCheck.Count > 0)
            {
                CreepPoint point = creepPoints[toCheck[0].x, toCheck[0].y, toCheck[0].z];

                checkedPoints.Add(toCheck[0]);
                toCheck.RemoveAt(0);

                if (point == null || !point.active)
                    continue;

                foreach (Vector3Int neighborIndex in neighbors[point.index].Where(p => !checkedPoints.Contains(p)))
                {
                    CreepPoint neighbor = creepPoints[neighborIndex.x, neighborIndex.y, neighborIndex.z];

                    if (neighbor == null || !neighbor.active || neighbor.GetConnectedNeighbors().Contains(point.index))
                        continue;

                    Vector3 pPos = point.worldPosition + point.normal * 0.05f,
                        nPos = neighbor.worldPosition + neighbor.normal * 0.05f;
                    Vector3 dir = nPos - pPos;

                    if (!Physics.Raycast(nPos, -dir.normalized, dir.magnitude, manager.GetBlockMask(),
                            QueryTriggerInteraction.Ignore) &&
                        !Physics.Raycast(pPos, dir.normalized, dir.magnitude, manager.GetBlockMask(),
                            QueryTriggerInteraction.Ignore))
                    {
                        if (!checkedPoints.Contains(neighbor.index) && !toCheck.Contains(neighbor.index))
                            toCheck.Add(neighbor.index);

                        point.AddConnected(neighborIndex);
                        neighbor.AddConnected(point.index);
                    }
                }

                i++;
                percent = (int) (i / (float) totalCount * 100);

                yield return 2;
            }

            foreach (CreepPoint creepPoint in CommonVariable.MultiDimensionalToList(creepPoints)
                         .Where(cp => cp != null && cp.active))
                creepPoint.active = creepPoint.GetConnectedNeighbors().Length >= 2;

            #endregion

            #region Save

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

                i++;

                if (i % 50 == 0)
                {
                    EditorUtility.SetDirty(managerSaveData);
                    yield return 5;
                }

                percent = (int) (i / (float) totalCount * 100);
            }

            EditorUtility.SetDirty(manager);

            #endregion

            generating = false;
        }
    }
}