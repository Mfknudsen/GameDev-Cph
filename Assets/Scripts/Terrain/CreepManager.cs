#region Packages

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameDev.Common;
using Photon.Pun;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

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
        [SerializeField] private Vector2 randomSpread = Vector2.zero;
        [SerializeField] private Material creepMaterial;
        [SerializeField] private float distanceOfSurface = 0.2f;
        [SerializeField] private AnimationCurve riseCurve;
        [SerializeField] private Transform closestValidPoint;

        [Header("Debug")] [SerializeField] private bool debugSquare;
        [SerializeField] private int[] toDeactivate;
        [SerializeField] private Transform checkFromPoint;

        [SerializeField] public List<CreepPoint> serializedList;

       [SerializeReference] public CreepPoint[,,] creepPoints;

        private List<CreepPoint> toUpdateSource = new List<CreepPoint>(),
            activeVertices = new List<CreepPoint>();

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
                    cube.AddPoint(creepPoint);
                    creepPoint.active = true;

                    creepPoint.vertIndex = i;
                    i++;

                    List<CreepPoint> toAdd = new List<CreepPoint>();
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

                                toAdd.Add(creepPoints[
                                    index.x,
                                    index.y,
                                    index.z]);
                                creepPoint.AddConnected(creepPoints[index.x,
                                    index.y,
                                    index.z]);
                            }
                        }
                    }

                    creepPoint.SetNeighbors(toAdd.Select(c => c.index).ToArray());

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
            
            //StartCoroutine(GeneratePoints());
        }

        private void Update()
        {
            if (creepPoints == null || creepPoints.Length == 0)
                return;

            if (debugSquare)
                return;

            if (ready && PhotonNetwork.IsMasterClient)
                UpdatePoints();

            verts.Clear();
            for (int i = 0; i < activeVertices.Count; i++)
            {
                CreepPoint point = activeVertices[i];
                verts.Add(point.worldPosition +
                          point.normal *
                          (riseCurve.Evaluate(point.GetSpread()) * distanceOfSurface *
                           (riseCurve.Evaluate(point.GetSpread()) > 0 ? point.GetNoise() : 1)));
            }

            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

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
            Gizmos.DrawWireCube(transform.position + (Vector3)fieldSize / 2, fieldSize);

            if (creepPoints == null)
                return;

            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (!creepPoint.active || creepPoint.GetConnectedNeighbors().Length == 0)
                    continue;

                Color c = Color.red;
                //c.a = creepPoint.GetSpread();
                Gizmos.color = c;

                Gizmos.DrawWireSphere(creepPoint.worldPosition, 0.25f / pointsPerAxis);

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
            if (activeVertices.Contains(point)) return;

            point.vertIndex = activeVertices.Count;

            activeVertices.Add(point);

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
            if (!activeVertices.Contains(point)) return;

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

            foreach (CreepPoint activeVertex in activeVertices)
            {
                CreepPoint creepPoint = activeVertex;

                if (activeVertex.vertIndex > point.vertIndex)
                    creepPoint.vertIndex--;

                Vector3Int pointIndex = creepPoint.index;
                creepPoints[pointIndex.x, pointIndex.y, pointIndex.z] = creepPoint;
            }
        }

        #endregion

        #region Internal

        private void UpdatePoints()
        {
            List<CreepPoint> toRemove = new List<CreepPoint>(),
                toAdd = new List<CreepPoint>();

            #region Job

            NativeArray<float> pointDataArray =
                new NativeArray<float>(toUpdateSource.Count, Allocator.TempJob);

            for (int i = 0; i < toUpdateSource.Count; i++)
            {
                pointDataArray[i] = toUpdateSource[i].GetSpread();
            }

            UpdatePointJob job = new UpdatePointJob()
            {
                pointDataArray = pointDataArray,
                deltaTime = Time.deltaTime,
                spreadPercentagePerSecond = spreadPercentagePerSecond,
                randomSpread = new float2(randomSpread.x, randomSpread.y),
                random = new Random(1)
            };

            JobHandle jobHandler = job.Schedule(pointDataArray.Length, 50);
            jobHandler.Complete();

            for (int i = 0; i < pointDataArray.Length; i++)
            {
                CreepPoint point = toUpdateSource[i];
                point.SetSpread(pointDataArray[i]);

                if (point.GetSpread().Equals(1f))
                    toRemove.Add(point);
                else if (point.GetSpread() > 0.5f)
                {
                    foreach (Vector3Int connectedIndex in point.GetConnectedNeighbors())
                    {
                        CreepPoint creepPoint = creepPoints[connectedIndex.x, connectedIndex.y, connectedIndex.z];
                        if (creepPoint.GetSpread() == 0 && !toUpdateSource.Contains(creepPoint))
                            toAdd.Add(creepPoint);
                    }
                }
            }

            pointDataArray.Dispose();

            #endregion

            toUpdateSource.AddRange(toAdd);
            toRemove.ForEach(e => toUpdateSource.Remove(e));
        }

        private IEnumerator GeneratePoints()
        {
            Debug.Log("Start");

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
                            origin.position + new Vector3(x, y, z) / pointsPerAxis,
                            this,
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
                    createOnMask
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

            serializedList = CommonVariable.MultiDimensionalToList(creepPoints);
            Debug.Log(serializedList.Count);
        }

        private IEnumerator UsablePoints()
        {
            List<Vector3Int> checkedPoints = new List<Vector3Int>(),
                toCheck = new List<Vector3Int>();

            toCheck.Add(CommonVariable.MultiDimensionalToList(creepPoints)
                .Where(cp => cp != null && cp.active)
                .OrderBy(cp => Vector3.Distance(cp.worldPosition, closestValidPoint.position)).First().index);

            while (toCheck.Count > 0)
            {
                CreepPoint point = creepPoints[toCheck[0].x, toCheck[0].y, toCheck[0].z];

                checkedPoints.Add(toCheck[0]);
                toCheck.RemoveAt(0);

                if (point == null || !point.active)
                    continue;

                foreach (Vector3Int neighborIndex in point.GetNeighbors()
                             .Where(i =>
                                 !checkedPoints.Contains(i) &&
                                 !point.GetConnectedNeighbors().Contains(i)))
                {
                    CreepPoint neighbor = creepPoints[neighborIndex.x, neighborIndex.y, neighborIndex.z];

                    Vector3 pPos = point.worldPosition + point.normal * 0.05f,
                        nPos = neighbor.worldPosition + neighbor.normal * 0.05f;
                    Vector3 dir = nPos - pPos;

                    if (!Physics.Raycast(neighbor.worldPosition, -dir.normalized, dir.magnitude, blockConnectionMask,
                            QueryTriggerInteraction.Ignore) &&
                        !Physics.Raycast(point.worldPosition, dir.normalized, dir.magnitude, blockConnectionMask,
                            QueryTriggerInteraction.Ignore))
                    {
                        if (!checkedPoints.Contains(neighbor.index) && !toCheck.Contains(neighbor.index))
                            toCheck.Add(neighbor.index);

                        point.AddConnected(neighbor);
                        neighbor.AddConnected(point);
                    }
                }

                yield return null;
            }

            foreach (CreepPoint creepPoint in CommonVariable.MultiDimensionalToList(creepPoints)
                         .Where(cp => cp != null && cp.active))
                creepPoint.active = creepPoint.GetConnectedNeighbors().Length > 0;

            #region Aveage normals

            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (creepPoint == null || !creepPoint.active)
                    continue;

                Vector3 total = Vector3.zero;
                int count = 0;
                foreach (Vector3Int neighborIndex in creepPoint.GetConnectedNeighbors()
                             .Where(i => i != creepPoint.index))
                {
                    total += creepPoints[neighborIndex.x, neighborIndex.y, neighborIndex.z].normal;
                    count++;
                }

                creepPoint.normal = (total / count).normalized;
            }

            #endregion

            #region Aveage position

            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (creepPoint == null || !creepPoint.active)
                    continue;

                Vector3 total = creepPoint.worldPosition;
                int count = 1;
                foreach (Vector3Int neighborIndex in creepPoint.GetConnectedNeighbors()
                             .Where(i => i != creepPoint.index))
                {
                    total += creepPoints[neighborIndex.x, neighborIndex.y, neighborIndex.z].worldPosition;
                    count++;
                }

                creepPoint.worldPosition = total / count;
            }

            #endregion

            if (checkFromPoint != null)
            {
                toUpdateSource.Add(CommonVariable.MultiDimensionalToList(creepPoints)
                    .Where(cp =>
                        cp != null && cp.active)
                    .OrderBy(cp => Vector3.Distance(cp.worldPosition, checkFromPoint.position)).First());
            }

            Debug.Log("Done");
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
                activeVertices.Remove(creepPoint);
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
        public float2 randomSpread;

        public Random random;

        #endregion

        #region In

        public void Execute(int index)
        {
            float currentSpread = pointDataArray[index];

            currentSpread = Mathf.Clamp(
                currentSpread + spreadPercentagePerSecond *
                random.NextFloat(randomSpread.x, randomSpread.y) *
                deltaTime,
                0,
                1
            );

            pointDataArray[index] = currentSpread;
        }

        #endregion
    }

    [BurstCompile]
    public struct UpdateVerticesPositionJob : IJobParallelFor
    {
        public void Execute(int index)
        {
        }
    }

    #endregion
}