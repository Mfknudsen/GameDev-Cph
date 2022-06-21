#region Packages

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameDev.Common;
using Photon.Pun;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

#endregion

namespace GameDev.Terrain.Creep
{
    public class CreepManager : MonoBehaviourPunCallbacks
    {
        #region Values

        public static CreepManager instance;

        [SerializeField] private Vector3Int fieldSize = Vector3Int.one;
        [SerializeField] private int pointsPerAxis = 1;
        [SerializeField] private LayerMask createOnMask, blockConnectionMask;
        [Range(0f, 1f)] [SerializeField] private float spreadPercentagePerSecond;
        [SerializeField] private Material creepMaterial;
        [SerializeField] private float distanceOfSurface = 0.2f;
        [SerializeField] private AnimationCurve riseCurve;
        [SerializeField] private Transform closestValidPoint;

        [Header("Debug")] [SerializeField] private bool debugSquare;
        [SerializeField] private int[] toDeactivate;

        [SerializeField] private CreepManagerSaveData saveData;
        [SerializeField] private int debugStrength;

        private CreepPoint[,,] creepPoints;

        private List<Vector3Int> toUpdateSource = new List<Vector3Int>(),
            activeVertices = new List<Vector3Int>();

        private bool ready;

        private MeshFilter meshFilter;
        private Mesh mesh;

        private List<Vector3> verts = new List<Vector3>();
        private List<int> tris = new List<int>();

        private List<Vector3Int> vertsAdd = new List<Vector3Int>(),
            vertsRemove = new List<Vector3Int>();

        private List<Triangle> activeTriangles = new List<Triangle>();

        #endregion

        #region Build In States

        private void Start()
        {
            if (instance != null)
                Destroy(gameObject);

            instance = this;

            if (meshFilter == null)
            {
                GameObject obj = new GameObject("Mesh");
                meshFilter = obj.AddComponent<MeshFilter>();
                obj.AddComponent<MeshRenderer>().material = creepMaterial;
                obj.transform.parent = transform;

                mesh = new Mesh();
                mesh.name = "Creep Mesh";
                meshFilter.mesh = mesh;
            }

            if (debugSquare)
            {
                GameObject.Find("Terrain").SetActive(false);

                creepPoints = new CreepPoint[2, 2, 2];

                Cube cube = new Cube();

                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int z = 0; z < 2; z++)
                        {
                            creepPoints[x, y, z] = new CreepPoint(new Vector3Int(x, y, z),
                                new Vector3(x, y, z), this, 1);
                        }
                    }
                }

                int i = 0;
                foreach (CreepPoint cp in creepPoints)
                {
                    CreepPoint creepPoint = cp;
                    cube.AddPoint(creepPoint, i);
                    creepPoint.active = true;

                    creepPoint.vertIndex = i;
                    i++;

                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            for (int z = -1; z < 2; z++)
                            {
                                if (new Vector3(x, y, z) == Vector3.zero)
                                    continue;

                                Vector3Int index = new Vector3Int(x, y, z) + creepPoint.index;

                                if (index.x < 0 || index.x >= creepPoints.GetLength(0) ||
                                    index.y < 0 || index.y >= creepPoints.GetLength(1) ||
                                    index.z < 0 || index.z >= creepPoints.GetLength(2))
                                    continue;

                                cp.AddConnected(index);
                                creepPoint.AddConnected(index);
                            }
                        }
                    }

                    creepPoint.normal = (creepPoint.worldPosition - new Vector3(0.5f, 0.5f, 0.5f)).normalized;

                    Vector3Int pointIndex = creepPoint.index;
                    creepPoints[pointIndex.x, pointIndex.y, pointIndex.z] = creepPoint;
                }

                List<CreepPoint> tempDeactivate = CommonVariable.MultiDimensionalToList(creepPoints);
                foreach (int index in toDeactivate)
                {
                    CreepPoint creepPoint = tempDeactivate[index + 1];
                    creepPoint.vertIndex = -1;
                    Vector3Int pointIndex = creepPoint.index;
                    creepPoints[pointIndex.x, pointIndex.y, pointIndex.z] = creepPoint;
                }

                foreach (CreepPoint creepPoint in creepPoints)
                {
                    if (creepPoint.active)
                        verts.Add(creepPoint.worldPosition);
                }

                tris.AddRange(cube.Calculate(null));

                mesh.vertices = verts.ToArray();
                mesh.triangles = tris.ToArray();
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                return;
            }

#if UNITY_EDITOR
            StartCoroutine(GeneratePoints());
            StartCoroutine(UpdateMesh());
#endif
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)fieldSize / 2, fieldSize);

            if (creepPoints == null)
                return;

            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (creepPoint is not { active: true })
                    continue;

                Color c = Color.red;
                //c.a = creepPoint.GetSpread();
                Gizmos.color = c;

                if (creepPoint.GetSpread() == 0)
                    continue;

                foreach (Vector3Int connectedNeighbor in creepPoint.GetConnectedNeighbors())
                {
                    Debug.DrawLine(creepPoint.worldPosition,
                        creepPoints[connectedNeighbor.x, connectedNeighbor.y, connectedNeighbor.z].worldPosition);
                }

                continue;
                Gizmos.DrawWireSphere(creepPoint.worldPosition + creepPoint.normal *
                    (riseCurve.Evaluate(creepPoint.GetSpread()) *
                     distanceOfSurface), 0.2f / pointsPerAxis);

                if (debugSquare)
                {
                    if (creepPoint.vertIndex < 0)
                        continue;
                    c = Color.green;
                    Gizmos.color = c;
                    Gizmos.DrawRay(creepPoint.worldPosition +
                                   creepPoint.normal * (riseCurve.Evaluate(creepPoint.GetSpread()) *
                                                        distanceOfSurface),
                        creepPoint.normal);

                    CommonDebug.DrawString((creepPoint.vertIndex - 1).ToString(), creepPoint.worldPosition, 30);
                }
            }
        }

        #endregion

        #region Getters

        public Vector3Int GetSize()
        {
            return fieldSize;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public int GetPointsPer()
        {
            return pointsPerAxis;
        }

        public LayerMask GetBuildMask()
        {
            return createOnMask;
        }

        public LayerMask GetBlockMask()
        {
            return blockConnectionMask;
        }

        public Vector3 GetClosestPointPosition()
        {
            return closestValidPoint.position;
        }

        public CreepManagerSaveData GetSaveDataHolder()
        {
            return saveData;
        }

        public bool GetIsReady()
        {
            return ready;
        }

        #endregion

        #region Setters

        public void SetPoints(CreepPoint[,,] set)
        {
            creepPoints = set;
        }

        #endregion

        #region In

        public void AddActivePoint(Vector3Int point)
        {
            if (!vertsAdd.Contains(point))
                vertsAdd.Add(point);
        }

        public void RemoveActivePoint(Vector3Int point)
        {
            if (!vertsRemove.Contains(point))
                vertsRemove.Add(point);
        }

        public void AddUpdatePoint(Vector3Int point, int spreadStrength)
        {
            if (!toUpdateSource.Contains(point))
                toUpdateSource.Add(point);

            creepPoints[point.x, point.y, point.z].AddStrength(spreadStrength - 1);
        }

        public void RemoveUpdatePoint(Vector3Int point)
        {
            toUpdateSource.Remove(point);
        }

        #endregion

        #region Out

        public CreepPoint GetClosestToPosition(Vector3 position)
        {
            return CommonVariable.MultiDimensionalToList(creepPoints)
                .Where(cp =>
                    cp != null && cp.active && Vector3.Angle(Vector3.up, cp.normal) < 45)
                .OrderBy(cp =>
                    Vector3.Distance(cp.worldPosition, position)).First();
        }

        #endregion

        #region Internal

        private IEnumerator GeneratePoints()
        {
            #region Setup

            Vector3 origin = transform.position;

            creepPoints = Array.CreateInstance(typeof(CreepPoint),
                Mathf.Abs(fieldSize.x) * pointsPerAxis,
                Mathf.Abs(fieldSize.y) * pointsPerAxis,
                Mathf.Abs(fieldSize.z) * pointsPerAxis) as CreepPoint[,,];

            int i = 0;
            foreach (CreepPointSaveData creepPointSaveData in saveData.pointSaveData)
            {
                Vector3Int index = creepPointSaveData.index;
                CreepPoint point = new CreepPoint(
                    index,
                    creepPointSaveData.world,
                    this,
                    Mathf.PerlinNoise((index.x + index.y) * 0.9f, (index.z + index.y) * 0.9f)
                );

                foreach (Vector3Int vector3Int in creepPointSaveData.connected)
                    point.AddConnected(vector3Int);

                point.active = true;
                point.normal = creepPointSaveData.normal;

                creepPoints[index.x, index.y, index.z] = point;

                i++;
                if (i % 500 == 0)
                    yield return null;
            }

            #endregion

            #region Setup Cubes

            for (int xIndex = 0; xIndex < creepPoints.GetLength(0); xIndex++)
            {
                for (int yIndex = 0; yIndex < creepPoints.GetLength(1); yIndex++)
                {
                    for (int zIndex = 0; zIndex < creepPoints.GetLength(2); zIndex++)
                    {
                        Vector3Int pointIndex = new Vector3Int(xIndex, yIndex, zIndex);
                        CreepPoint creepPoint = creepPoints[xIndex, yIndex, zIndex];

                        Cube cube = new Cube();
                        cube.AddPoint(creepPoint, 0);

                        int index = 1;
                        for (int x = 0; x < 2; x++)
                        {
                            for (int y = 0; y < 2; y++)
                            {
                                for (int z = 0; z < 2; z++)
                                {
                                    Vector3Int total = new Vector3Int(x, y, z) + pointIndex;

                                    if (total == pointIndex ||
                                        total.x < 0 || total.x >= creepPoints.GetLength(0) ||
                                        total.y < 0 || total.y >= creepPoints.GetLength(1) ||
                                        total.z < 0 || total.z >= creepPoints.GetLength(2))
                                        continue;

                                    CreepPoint point = creepPoints[total.x, total.y, total.z];

                                    cube.AddPoint(point, index);
                                    index++;
                                }
                            }
                        }

                        i++;
                        if (i % 500 == 0)
                            yield return null;
                    }
                }
            }

            #endregion

            #region Aveage normals

            foreach (CreepPoint creepPoint in CommonVariable.MultiDimensionalToList(creepPoints)
                         .Where(cp => cp != null && cp.active))
            {
                Vector3 total = Vector3.zero;

                foreach (Vector3Int connected in creepPoint.GetConnectedNeighbors())
                    total = total + creepPoints[connected.x, connected.y, connected.z].normal;

                creepPoint.normal = total / creepPoint.GetConnectedNeighbors().Length;
            }

            #endregion

            ready = true;

            CreepPoint p = CommonVariable.MultiDimensionalToList(creepPoints)
                .Where(e => e != null)
                .OrderBy(e => Vector3.Distance(e.worldPosition, closestValidPoint.position)).First();

            p.spreadStrength.Add(debugStrength);
            p.SetSpread(.75f);

            closestValidPoint.position = p.worldPosition;

            StartCoroutine(UpdatePoints());
        }

        private IEnumerator UpdatePoints()
        {
            yield return new WaitWhile(() => !ready);

            while (true)
            {
                #region Job

                Vector3Int[] readUpdateSource = new Vector3Int[toUpdateSource.Count];
                toUpdateSource.CopyTo(readUpdateSource);

                NativeArray<float> pointDataArray =
                    new NativeArray<float>(readUpdateSource.Length, Allocator.TempJob);

                for (int i = 0; i < pointDataArray.Length; i++)
                {
                    Vector3Int index = readUpdateSource[i];
                    pointDataArray[i] = creepPoints[index.x, index.y, index.z]
                        .GetSpread();
                }

                float deltaTime = Time.deltaTime;
                UpdatePointJob job = new UpdatePointJob(
                    pointDataArray,
                    ref deltaTime,
                    ref spreadPercentagePerSecond);

                JobHandle jobHandler = job.Schedule(pointDataArray.Length, 50);
                jobHandler.Complete();

                for (int i = 0; i < pointDataArray.Length; i++)
                {
                    Vector3Int index = readUpdateSource[i];
                    creepPoints[index.x, index.y, index.z]
                        .SetSpread(pointDataArray[i]);
                }

                pointDataArray.Dispose();

                #endregion

                yield return null;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private IEnumerator UpdateMesh()
        {
            yield return new WaitWhile(() => !ready);

            while (true)
            {
                int offset = 0;
                if (vertsRemove.Count > 0)
                {
                    int[] readRemove =
                        vertsRemove.Select(i =>
                                creepPoints[i.x, i.y, i.z].vertIndex).OrderBy(i => i).Reverse()
                            .ToArray();
                    vertsRemove.Clear();

                    List<int> trianglesToRemove = new List<int>();
                    foreach (int vertIndex in readRemove)
                    {
                        activeVertices.RemoveAt(vertIndex);

                        for (int i = activeTriangles.Count - 1; i >= 0; i--)
                        {
                            if (trianglesToRemove.Contains(i)) continue;

                            if (activeTriangles[i].GetAsArray().Contains(vertIndex))
                                trianglesToRemove.Add(i);
                        }

                        offset = offset + 1;
                        if (offset % 50 == 0)
                            yield return 1;
                    }

                    trianglesToRemove.ForEach(i => activeTriangles.RemoveAt(i));

                    for (int i = 0; i < activeVertices.Count; i++)
                    {
                        Vector3Int tempIndex = activeVertices[i];
                        creepPoints[tempIndex.x, tempIndex.y, tempIndex.z].vertIndex = i;

                        offset = offset + 1;
                        if (offset % 50 == 0)
                            yield return 1;
                    }
                }

                offset = 0;
                if (vertsAdd.Count > 0)
                {
                    Vector3Int[] readAdd = new Vector3Int[vertsAdd.Count];
                    vertsAdd.CopyTo(readAdd);
                    vertsAdd.Clear();

                    foreach (Vector3Int index in readAdd)
                    {
                        if (activeVertices.Contains(index)) continue;

                        activeVertices.Add(index);
                        CreepPoint added = creepPoints[index.x, index.y, index.z];
                        added.vertIndex = activeVertices.Count - 1;

                        foreach (Cube cube in added.cubesAffected)
                        {
                            activeTriangles.AddRange(
                                Triangle.CreateFromArray(cube.Calculate(added)
                                    .Select(i =>
                                        activeVertices[i])
                                    .Select(i =>
                                        creepPoints[i.x, i.y, i.z])
                                    .ToArray()));

                            offset = offset + 1;
                        }

                        if (offset % 50 == 0)
                            yield return 1;
                    }

                    yield return 1;
                }

                #region Mesh

                if (activeVertices.Count > 2)
                {
                    #region Job

                    NativeArray<Vector3> worldDataArray =
                            new NativeArray<Vector3>(activeVertices.Count, Allocator.TempJob),
                        normalDataArray = new NativeArray<Vector3>(activeVertices.Count, Allocator.TempJob);

                    NativeArray<float> riseDataArray = new NativeArray<float>(activeVertices.Count, Allocator.TempJob),
                        noiseDataArray = new NativeArray<float>(activeVertices.Count, Allocator.TempJob);

                    NativeArray<bool> edgeDataArray = new NativeArray<bool>(activeVertices.Count, Allocator.TempJob);

                    for (int i = 0; i < activeVertices.Count; i++)
                    {
                        Vector3Int index = activeVertices[i];
                        CreepPoint creepPoint = creepPoints[index.x, index.y, index.z];

                        worldDataArray[i] = creepPoint.worldPosition;
                        normalDataArray[i] = creepPoint.normal;
                        riseDataArray[i] = riseCurve.Evaluate(creepPoint.GetSpread());
                        noiseDataArray[i] = creepPoint.GetNoise();
                        edgeDataArray[i] = creepPoint.spreadStrength[0] > 0;
                    }

                    UpdateVertPositionJob vertPositionJob = new UpdateVertPositionJob()
                    {
                        worldDataArray = worldDataArray,
                        normalDataArray = normalDataArray,
                        riseDataArray = riseDataArray,
                        noiseDataArray = noiseDataArray,
                        distanceFromSurface = distanceOfSurface,
                        edgeDataArray = edgeDataArray
                    };

                    yield return 1;

                    JobHandle jobHandle = vertPositionJob.Schedule(worldDataArray.Length, 50);
                    jobHandle.Complete();

                    yield return 1;

                    Vector3[] calculatedVerts = new Vector3[worldDataArray.Length];
                    for (int i = 0; i < worldDataArray.Length; i++)
                        calculatedVerts[i] = worldDataArray[i];

                    worldDataArray.Dispose();
                    normalDataArray.Dispose();
                    riseDataArray.Dispose();
                    noiseDataArray.Dispose();
                    edgeDataArray.Dispose();

                    yield return 1;

                    #endregion

                    mesh.vertices = calculatedVerts;

                    List<int> tries = new List<int>();
                    activeTriangles.ForEach(t => tries.AddRange(t.GetAsArray()));
                    mesh.triangles = tries.ToArray();
                    mesh.RecalculateNormals();
                }

                yield return 5;

                #endregion
            }
            // ReSharper disable once IteratorNeverReturns
        }

        #region PunRPC

        [PunRPC]
        // ReSharper disable once UnusedMember.Local
        private void RPCReceiveTriangleUpdate(Vector3Int[] vertAdd, Vector3Int[] vertRemove, int[] triesAdd,
            int[] triesRemove)
        {
            foreach (CreepPoint creepPoint in CommonVariable.MultiDimensionalToList(creepPoints)
                         .Where(cp => vertRemove.Contains(cp.index)).OrderBy(cp => cp.vertIndex).Reverse())
            {
                activeVertices.Remove(creepPoint.index);
            }

            foreach (Vector3Int index in vertAdd)
                verts.Add(creepPoints[index.x, index.y, index.z].worldPosition);

            foreach (int i in triesRemove.OrderBy(i => i).Reverse())
                tris.RemoveAt(i);

            tris.AddRange(triesAdd);
        }

        #endregion

        #endregion
    }

    #region Jobs

    [BurstCompile]
    public struct UpdatePointJob : IJobParallelFor
    {
        #region Values

        public NativeArray<float> pointDataArray;
        public float deltaTime, spreadPercentagePerSecond;

        #endregion

        #region Build In States

        public UpdatePointJob(NativeArray<float> pointDataArray, ref float deltaTime,
            ref float spreadPercentagePerSecond)
        {
            this.pointDataArray = pointDataArray;
            this.deltaTime = deltaTime;
            this.spreadPercentagePerSecond = spreadPercentagePerSecond;
        }

        #endregion

        #region In

        public void Execute(int index)
        {
            float currentSpread = pointDataArray[index];

            currentSpread = Mathf.Clamp(
                currentSpread + spreadPercentagePerSecond * deltaTime,
                0,
                1
            );

            pointDataArray[index] = currentSpread;
        }

        #endregion
    }

    [BurstCompile]
    public struct UpdateVertPositionJob : IJobParallelFor
    {
        public NativeArray<Vector3> worldDataArray, normalDataArray;
        public NativeArray<float> riseDataArray, noiseDataArray;
        public NativeArray<bool> edgeDataArray;

        public float distanceFromSurface;

        public void Execute(int index)
        {
            float visualSpread = edgeDataArray[index] ? riseDataArray[index] : -1.5f;

            worldDataArray[index] = worldDataArray[index] + normalDataArray[index] *
                (visualSpread * noiseDataArray[index] * distanceFromSurface);
        }
    }

    #endregion

    public struct Triangle
    {
        private CreepPoint i1, i2, i3;

        public Triangle(CreepPoint i1, CreepPoint i2, CreepPoint i3)
        {
            this.i1 = i1;
            this.i2 = i2;
            this.i3 = i3;
        }

        public int[] GetAsArray()
        {
            return new[] { i1.vertIndex, i2.vertIndex, i3.vertIndex };
        }

        public static Triangle[] CreateFromArray(CreepPoint[] arr)
        {
            List<Triangle> result = new List<Triangle>();

            for (int i = 0; i < arr.Length; i += 3)
            {
                if (i + 2 > arr.Length - 1)
                    break;

                result.Add(new Triangle(arr[i], arr[i + 1], arr[i + 2]));
            }

            return result.ToArray();
        }
    }
}