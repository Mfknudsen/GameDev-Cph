#region Packages

using System.Collections;
using System.Collections.Generic;
using GameDev.Common;
using GameDev.Editor.Utilities;
using GameDev.Terrain;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

#endregion

namespace GameDev.Editor.Terrain
{
    [CustomEditor(typeof(CreepManager))]
    public class CreepManagerEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (EditorGUILayout.Toggle(false))
            {
                CreepManager manager = (CreepManager)target;
                EditorCoroutines.Execute(GeneratePoints(manager, manager.GetPosition(), manager.GetSize(),
                    manager.GetPointsPer()));
            }
        }

        private static IEnumerator GeneratePoints(CreepManager manager, Vector3 origin, Vector3Int fieldSize,
            int pointsPerAxis)
        {
            Debug.Log("Start");

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

            Debug.Log("Setup");

            #endregion

            #region Set neighbors

            int i = 0;
            foreach (CreepPoint creepPoint in creepPoints)
            {
                Vector3Int index = creepPoint.index;

                if (index.x == creepPoints.GetLength(0) - 1 ||
                    index.y == creepPoints.GetLength(1) - 1 ||
                    index.z == creepPoints.GetLength(2) - 1)
                    continue;

                List<Vector3Int> toSet = new List<Vector3Int>();

                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        for (int z = -1; z < 2; z++)
                        {
                            Vector3Int toLook = index + new Vector3Int(x, y, z);

                            if (toLook.x < 0 || toLook.y < 0 || toLook.z < 0)
                                continue;

                            if (new Vector3(x, y, z) == Vector3.zero)
                                continue;

                            toSet.Add(toLook);
                        }
                    }
                }

                creepPoint.SetNeighbors(toSet.ToArray());

                i++;

                if (i % 1000 == 0)
                    yield return null;
            }

            #endregion

            Debug.Log("Neighbors");

            #region Point is surface

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
                    creepPoint.worldPosition = closestPoint + creepPoint.normal * 0.03f;
                }

                i++;
                if (i % 1000 == 0)
                    yield return null;
            }

            #endregion

            Debug.Log("Surface");

            #region Setup Cubes

            foreach (CreepPoint creepPoint in creepPoints)
            {
                Cube cube = new Cube();
                cube.AddPoint(creepPoint);

                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int z = 0; z < 2; z++)
                        {
                            if (new Vector3(x, y, z) == Vector3.zero) continue;

                            Vector3Int total = new Vector3Int(x, y, z) + creepPoint.index;

                            if (total.x >= creepPoints.GetLength(0) ||
                                total.y >= creepPoints.GetLength(1) ||
                                total.z >= creepPoints.GetLength(2))
                                continue;

                            cube.AddPoint(creepPoints[total.x, total.y, total.z]);
                        }
                    }
                }
            }

            #endregion

            Debug.Log("Cube");

            i = 0;
            manager.serializedList = new List<CreepPoint>();
            foreach (CreepPoint creepPoint in creepPoints)
            {
                Undo.RecordObject(manager, "Generating Points");

                manager.serializedList.Add(creepPoint);

                EditorUtility.SetDirty(manager);
                
                i++;
                if (i % 50 == 0)
                    yield return null;
            }

            Debug.Log("Done");
        }
    }
}