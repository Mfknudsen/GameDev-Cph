#region Packages

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameDev.Common;
using GameDev.Terrain.Creep;
using UnityEditor;
using UnityEngine;

#endregion

namespace GameDev.Editor
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
                    CreepManager manager = (CreepManager)target;
                    EditorCoroutines.Execute(GeneratePoints(manager, manager.GetPosition(), manager.GetSize(),
                        manager.GetPointsPer()));
                }
            }
            else
            {
                Rect r = EditorGUILayout.BeginVertical();
                GUILayout.Space(5);
                EditorGUI.ProgressBar(r, (float)percent / 100, currentPart);
                GUILayout.Space(20);
                EditorGUILayout.EndVertical();
            }
        }

        private IEnumerator GeneratePoints(CreepManager manager, Vector3 origin, Vector3Int fieldSize,
            int pointsPerAxis)
        {
            #region Setup Points

            CreepPoint[,,] creepPoints = Array.CreateInstance(typeof(CreepPoint),
                Mathf.Abs(fieldSize.x) * pointsPerAxis,
                Mathf.Abs(fieldSize.y) * pointsPerAxis,
                Mathf.Abs(fieldSize.z) * pointsPerAxis) as CreepPoint[,,];

            Debug.Log(creepPoints.Length);
            Dictionary<Vector3Int, Vector3Int[]> neighbors = new Dictionary<Vector3Int, Vector3Int[]>();
            int i = 0;

            currentPart = "Setup";
            int totalCount = creepPoints.Length;

            for (int x = 0; x < Mathf.Abs(fieldSize.x) * pointsPerAxis; x++)
            {
                for (int y = 0; y < Mathf.Abs(fieldSize.y) * pointsPerAxis; y++)
                {
                    for (int z = 0; z < Mathf.Abs(fieldSize.z) * pointsPerAxis; z++)
                    {
                        creepPoints[x, y, z] = new CreepPoint(
                            new Vector3Int(x, y, z),
                            origin + new Vector3(x, y, z) / pointsPerAxis,
                            manager,
                            0
                        );

                        i++;
                        if (i % 5000 == 0)
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
                    creepPoint.worldPosition = closestPoint + creepPoint.normal * 0.25f;
                }
                else
                {
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            for (int z = -1; z < 2; z++)
                            {
                                if (x == 0 && y == 0 && z == 0)
                                    continue;

                                Ray ray = new Ray(creepPoint.worldPosition,
                                    new Vector3(x, y, z).normalized / pointsPerAxis);
                                if (Physics.Raycast(ray, out RaycastHit hit, manager.GetBuildMask()))
                                {
                                    creepPoint.active = true;
                                    creepPoint.normal = hit.normal;
                                    creepPoint.worldPosition = hit.point + creepPoint.normal * 0.25f;

                                    break;
                                }
                            }

                            if (creepPoint.active)
                                break;
                        }

                        if (creepPoint.active)
                            break;
                    }
                }

                i++;
                percent = (int)(i / (float)totalCount * 100);

                if (i % 500 == 0)
                    yield return null;
            }

            #endregion

            #region Neighbors

            currentPart = "Neighbors";

            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (creepPoint is not { active: true })
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
                percent = (int)(i / (float)totalCount * 100);

                if (i % 500 == 0)
                    yield return null;
            }

            #endregion

            #region Usable

            currentPart = "Usable";

            i = 0;
            List<Vector3Int> checkedPoints = new List<Vector3Int>(),
                toCheck = new List<Vector3Int>();

            Debug.Log(CommonVariable.MultiDimensionalToList(creepPoints).Count(e => e.active));
            toCheck.Add(CommonVariable.MultiDimensionalToList(creepPoints)
                .Where(cp => cp is { active: true })
                .OrderBy(cp => Vector3.Distance(cp.worldPosition, manager.GetClosestPointPosition()))
                .First().index);

            totalCount = CommonVariable.MultiDimensionalToList(creepPoints)
                .Where(cp => cp is { active: true })
                .OrderBy(cp => Vector3.Distance(cp.worldPosition, manager.GetClosestPointPosition()))
                .Count();

            while (toCheck.Count > 0)
            {
                CreepPoint point = creepPoints[toCheck[0].x, toCheck[0].y, toCheck[0].z];

                checkedPoints.Add(toCheck[0]);
                toCheck.RemoveAt(0);

                if (point is not { active: true })
                    continue;

                foreach (Vector3Int neighborIndex in neighbors[point.index].Where(p => !checkedPoints.Contains(p)))
                {
                    CreepPoint neighbor = creepPoints[neighborIndex.x, neighborIndex.y, neighborIndex.z];

                    if (neighbor is not { active: true } || neighbor.GetConnectedNeighbors().Contains(point.index))
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
                percent = (int)(i / (float)totalCount * 100);


                if (i % 100 == 0)
                    yield return null;
            }

            foreach (CreepPoint creepPoint in CommonVariable.MultiDimensionalToList(creepPoints)
                         .Where(cp => cp is { active: true }))
                creepPoint.active = creepPoint.GetConnectedNeighbors().Length >= 2;

            #endregion

            #region Save

            currentPart = "Saving Data";

            totalCount = CommonVariable.MultiDimensionalToList(creepPoints)
                .Count(cp => cp is { active: true });

            CreepManagerSaveData managerSaveData = manager.GetSaveDataHolder();
            managerSaveData.pointSaveData = new List<CreepPointSaveData>();
            foreach (CreepPoint creepPoint in CommonVariable.MultiDimensionalToList(creepPoints)
                         .Where(cp => cp is { active: true }))
            {
                CreepPointSaveData pointSaveData = new CreepPointSaveData()
                {
                    index = creepPoint.index,
                    connected = creepPoint.GetConnectedNeighbors().ToList(),
                    normal = creepPoint.normal,
                    world = creepPoint.worldPosition
                };
                managerSaveData.pointSaveData.Add(pointSaveData);

                i++;

                if (i % 50 == 0)
                {
                    EditorUtility.SetDirty(managerSaveData);
                    yield return 5;
                }

                percent = (int)(i / (float)totalCount * 100);
            }

            EditorUtility.SetDirty(manager);

            #endregion

            generating = false;

            Debug.Log("Done Generating: " + CommonVariable.MultiDimensionalToList(creepPoints).Count(e => e.active));
        }
    }
}