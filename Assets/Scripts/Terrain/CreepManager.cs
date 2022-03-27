#region Packages

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

namespace GameDev.Terrain
{
    public class CreepManager : MonoBehaviourPunCallbacks
    {
        #region Values

        [SerializeField] private PhotonView pv;
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
        [SerializeField] private Transform checkFromPoint;

        [SerializeField] private CreepManagerSaveData saveData;

        [SerializeField, HideInInspector] public List<int> ownedNeighbor = new List<int>();

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

        private List<int> triRemove = new List<int>(),
            triAdd = new List<int>();

        #endregion

        #region Build In States

        private void Start()
        {
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

                tris.AddRange(cube.Calculate());

                mesh.vertices = verts.ToArray();
                mesh.triangles = tris.ToArray();
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                return;
            }

            StartCoroutine(GeneratePoints());

#if UNITY_EDITOR
            StartCoroutine(UpdatePoints());
#else
            if (PhotonNetwork.IsMasterClient)
                StartCoroutine(UpdatePoints());
#endif
        }

        private void Update()
        {
            if (creepPoints == null || creepPoints.Length == 0)
                return;

            if (debugSquare)
                return;

            verts.Clear();
            for (int i = 0; i < activeVertices.Count; i++)
            {
                CreepPoint point = creepPoints[activeVertices[i].x, activeVertices[i].y, activeVertices[i].z];
                verts.Add(point.worldPosition +
                          point.normal *
                          (riseCurve.Evaluate(point.GetSpread()) * distanceOfSurface *
                           (riseCurve.Evaluate(point.GetSpread()) > 0 ? point.GetNoise() : 1)));
            }

            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            List<Vector3Int> toRemove = new List<Vector3Int>(),
                toAdd = new List<Vector3Int>();
            foreach (Vector3Int index in toUpdateSource)
            {
                CreepPoint creepPoint = creepPoints[index.x, index.y, index.z];
                float spread = creepPoint.GetSpread();

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (spread == 1f)
                    toRemove.Add(index);

                if (spread > 0.5f)
                {
                    foreach (Vector3Int connectedNeighbor in creepPoint.GetConnectedNeighbors()
                                 .Where(cp => !toAdd.Contains(cp) && !toRemove.Contains(cp)))
                    {
                        if (creepPoints[connectedNeighbor.x, connectedNeighbor.y, connectedNeighbor.z].GetSpread() ==
                            0f)
                            toAdd.Add(connectedNeighbor);
                    }
                }
            }

            toRemove.ForEach(cp => toUpdateSource.Remove(cp));
            toUpdateSource.AddRange(toAdd);

            if (!PhotonNetwork.IsMasterClient) return;

            pv.RPC("RPCReceiveTriangleUpdate", RpcTarget.Others,
                vertsAdd.ToArray(),
                vertsRemove.ToArray(),
                triAdd.ToArray(),
                triRemove.ToArray());

            vertsAdd.Clear();
            vertsRemove.Clear();
            triAdd.Clear();
            triRemove.Clear();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + (Vector3) fieldSize / 2, fieldSize);

            if (creepPoints == null)
                return;

            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (creepPoint == null || !creepPoint.active || creepPoint.GetConnectedNeighbors().Length == 0)
                    continue;

                Color c = Color.red;
                c.a = creepPoint.GetSpread();
                Gizmos.color = c;

                Gizmos.DrawWireSphere(creepPoint.worldPosition, 0.2f / pointsPerAxis);

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

        #endregion

        #region Setters

        public void SetPoints(CreepPoint[,,] set)
        {
            creepPoints = set;
        }

        #endregion

        #region In

        public void AddPointAndUpdateTriangles(CreepPoint point)
        {
            if (activeVertices.Contains(point.index)) return;

            point.vertIndex = activeVertices.Count;

            activeVertices.Add(point.index);

            verts.Add(point.worldPosition);
            vertsAdd.Add(point.index);

            foreach (Cube cube in point.cubesAffected)
            {
                foreach (Vector3Int treePointIndex in cube.triangleIndexesOwned)
                {
                    for (int i = 0; i < tris.Count - 2; i++)
                    {
                        if (tris[i] == treePointIndex.x &&
                            tris[i + 1] == treePointIndex.y &&
                            tris[i + 2] == treePointIndex.z)
                        {
                            tris.RemoveAt(i + 2);
                            tris.RemoveAt(i + 1);
                            tris.RemoveAt(i);

                            triRemove.Add(i + 2);
                            triRemove.Add(i + 1);
                            triRemove.Add(i);

                            break;
                        }
                    }
                }

                int[] toAdd = cube.Calculate();
                cube.triangleIndexesOwned.Clear();
                for (int i = 0; i < toAdd.Length; i += 3)
                    cube.triangleIndexesOwned.Add(new Vector3Int(toAdd[i], toAdd[i + 1], toAdd[i + 2]));

                tris.AddRange(toAdd);
                triAdd.AddRange(toAdd);
            }
        }

        public void RemovePointAndUpdateList(CreepPoint point)
        {
            if (!activeVertices.Contains(point.index)) return;

            List<int> indexes = new List<int>();

            for (int i = 0; i < tris.Count; i++)
            {
                if (tris[i] == point.vertIndex)
                    indexes.Add(i);
            }

            for (int i = 0; i < indexes.Count; i++)
            {
                int offset = i * 3;
                switch ((indexes[i] + 1) % 3)
                {
                    case 0:
                        tris.RemoveAt(indexes[i] - offset);
                        tris.RemoveAt(indexes[i] - 1 - offset);
                        tris.RemoveAt(indexes[i] - 2 - offset);
                        break;

                    case 1:
                        tris.RemoveAt(indexes[i] + 2 - offset);
                        tris.RemoveAt(indexes[i] + 1 - offset);
                        tris.RemoveAt(indexes[i] - offset);
                        break;

                    case 2:
                        tris.RemoveAt(indexes[i] + 1 - offset);
                        tris.RemoveAt(indexes[i] - offset);
                        tris.RemoveAt(indexes[i] - 1 - offset);
                        break;
                }
            }

            for (int i = 0; i < tris.Count; i++)
            {
                if (tris[i] > point.vertIndex)
                    tris[i]--;
            }

            foreach (Vector3Int activeVertex in activeVertices)
            {
                CreepPoint creepPoint = creepPoints[activeVertex.x, activeVertex.y, activeVertex.z];

                if (creepPoint.vertIndex > point.vertIndex)
                    creepPoint.vertIndex--;

                Vector3Int pointIndex = creepPoint.index;
                creepPoints[pointIndex.x, pointIndex.y, pointIndex.z] = creepPoint;
            }
        }

        #endregion

        #region Internal

        private IEnumerator UpdatePoints()
        {
            while (!ready)
                yield return null;

            while (true)
            {
                #region Job

                NativeArray<float> pointDataArray =
                    new NativeArray<float>(toUpdateSource.Count, Allocator.TempJob);

                for (int i = 0; i < pointDataArray.Length; i++)
                {
                    pointDataArray[i] = creepPoints[toUpdateSource[i].x, toUpdateSource[i].y, toUpdateSource[i].z]
                        .GetSpread();
                }

                float deltaTime = Time.deltaTime;
                UpdatePointJob job = new UpdatePointJob(
                    ref pointDataArray,
                    ref deltaTime,
                    ref spreadPercentagePerSecond);

                JobHandle jobHandler = job.Schedule(pointDataArray.Length, 1);
                jobHandler.Complete();

                for (int i = 0; i < pointDataArray.Length; i++)
                    creepPoints[toUpdateSource[i].x, toUpdateSource[i].y, toUpdateSource[i].z]
                        .SetSpread(pointDataArray[i]);

                pointDataArray.Dispose();

                #endregion

                yield return null;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private IEnumerator GeneratePoints()
        {
            Vector3 origin = transform.position;
            creepPoints = new CreepPoint[fieldSize.x * pointsPerAxis, fieldSize.y * pointsPerAxis,
                fieldSize.z * pointsPerAxis];

            int i = 0;
            foreach (CreepPointSaveData creepPointSaveData in saveData.pointSaveData)
            {
                Vector3Int index = creepPointSaveData.index;
                CreepPoint point = new CreepPoint(
                    index,
                    origin + (Vector3) index / pointsPerAxis,
                    this,
                    Mathf.PerlinNoise((index.x + index.y) * 0.9f, (index.z + index.y) * 0.9f)
                );

                foreach (Vector3Int connectedIndex in creepPointSaveData.connected)
                    point.AddConnected(connectedIndex);

                point.active = true;

                creepPoints[index.x, index.y, index.z] = point;

                i++;
                if (i % 500 == 0)
                    yield return null;
            }

            #region Point surface

            i = 0;
            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (creepPoint == null || !creepPoint.active)
                    continue;

                Collider col = CommonPhysic.GetNearestSurfaceBySphere(
                    creepPoint.worldPosition,
                    1.25f / pointsPerAxis,
                    createOnMask
                );

                creepPoint.active = col != null;

                if (col != null)
                {
                    i++;
                    Vector3 closestPoint = col.ClosestPoint(creepPoint.worldPosition);

                    creepPoint.normal = (creepPoint.worldPosition - closestPoint).normalized;
                    creepPoint.worldPosition = closestPoint + creepPoint.normal * 0.03f;
                }

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

            if (checkFromPoint != null)
            {
                toUpdateSource.Add(CommonVariable.MultiDimensionalToList(creepPoints)
                    .Where(cp =>
                        cp != null && cp.active)
                    .OrderBy(cp => Vector3.Distance(cp.worldPosition, checkFromPoint.position))
                    .First().index);
            }

            ready = true;
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

        #region In Editor

        public void BuildPointGrid()
        {
            StartCoroutine(GeneratePoints());
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

        public UpdatePointJob(ref NativeArray<float> pointDataArray, ref float deltaTime,
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

    #endregion
}