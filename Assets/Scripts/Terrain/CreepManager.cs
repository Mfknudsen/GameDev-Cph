#region Packages

using System.Collections;
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

        [SerializeField] private bool DEBUG;
        [SerializeField] private int[] toDeactivate;

        [SerializeField] private Vector3Int fieldSize = Vector3Int.one;
        [SerializeField] private int pointsPerAxis = 1;
        [SerializeField] private LayerMask mask;
        [SerializeField] private Transform checkFromPoint;
        [Range(0f, 1f)] [SerializeField] private float spreadPercentagePerSecond;
        [SerializeField] private Vector2 randomSpread = Vector2.zero;
        [SerializeField] private Material creepMaterial;
        [SerializeField] private float distanceOfSurface = 0.2f;
        [SerializeField] private AnimationCurve riseCurve;

        private CreepPoint[,,] creepPoints;

        private List<CreepPoint> toUpdateSource = new List<CreepPoint>(),
            activeVertices = new List<CreepPoint>();

        private bool ready;

        private MeshFilter meshFilter;
        private Mesh mesh;

        private List<Vector3> verts = new List<Vector3>();
        private List<int> tris = new List<int>();

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

            if (DEBUG)
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
                                new Vector3(x, y, z), this);
                        }
                    }
                }

                int i = 0;
                foreach (CreepPoint creepPoint in creepPoints)
                {
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

                    creepPoint.SetNeighbors(toAdd.ToArray());

                    creepPoint.normal = (creepPoint.worldPosition - new Vector3(0.5f, 0.5f, 0.5f)).normalized;
                }

                List<CreepPoint> tempDeactivate = CommonVariable.MultiDimensionalToList(creepPoints);
                foreach (int index in toDeactivate)
                    tempDeactivate[index + 1].vertIndex = -1;

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

            GeneratePoints();

            toUpdateSource.Add(CommonVariable.MultiDimensionalToList(creepPoints).Where(cp => cp != null && cp.active)
                .OrderBy(cp => Vector3.Distance(cp.worldPosition, checkFromPoint.position)).First());

            foreach (CreepPoint creepPoint in toUpdateSource)
                creepPoint.SetSpread(1);

            StartCoroutine(UsablePoints());
        }

        private void Update()
        {
            if (DEBUG)
                return;

            if (!ready) return;

            UpdatePoints();

            verts.Clear();
            for (int i = 0; i < activeVertices.Count; i++)
            {
                CreepPoint point = activeVertices[i];
                verts.Add(point.worldPosition +
                          point.normal * (riseCurve.Evaluate(point.GetSpread()) *
                                          distanceOfSurface));
            }

            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)fieldSize / 2, fieldSize);

            if (creepPoints == null)
                return;

            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (creepPoint == null || !creepPoint.active)
                    continue;

                Color c = Color.red;
                //c.a = creepPoint.GetSpread();
                Gizmos.color = c;

                foreach (CreepPoint connectedNeighbor in creepPoint.GetConnectedNeighbors()
                             .Where(cp => cp != null && cp.active))
                    Gizmos.DrawLine(creepPoint.worldPosition + creepPoint.normal *
                        (riseCurve.Evaluate(creepPoint.GetSpread()) *
                         distanceOfSurface),
                        connectedNeighbor.worldPosition +
                        connectedNeighbor.normal * (riseCurve.Evaluate(connectedNeighbor.GetSpread()) *
                                                    distanceOfSurface));

                if (DEBUG)
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

        #region In

        public void AddAndUpdatePoint(CreepPoint point)
        {
            if (activeVertices.Contains(point)) return;

            point.vertIndex = activeVertices.Count;

            activeVertices.Add(point);

            verts.Add(point.worldPosition);

            foreach (Cube cube in point.cubesAffected)
            {
                tris.AddRange(cube.Calculate());
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
                if (activeVertex.vertIndex > point.vertIndex)
                    activeVertex.vertIndex--;
            }
        }

        #endregion

        #region Internal

        private void GeneratePoints()
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
                            origin.position + new Vector3(x, y, z) / pointsPerAxis,
                            this
                        );
                    }
                }
            }

            #endregion

            #region Set neighbors

            foreach (CreepPoint creepPoint in creepPoints)
            {
                Vector3Int index = creepPoint.index;

                if (index.x == creepPoints.GetLength(0) - 1 ||
                    index.y == creepPoints.GetLength(1) - 1 ||
                    index.z == creepPoints.GetLength(2) - 1)
                    continue;

                List<CreepPoint> toSet = new List<CreepPoint>();

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

                            toSet.Add(creepPoints[toLook.x, toLook.y, toLook.z]);
                        }
                    }
                }

                creepPoint.SetNeighbors(toSet.ToArray());
            }

            #endregion

            #region Point is surface

            foreach (CreepPoint creepPoint in creepPoints)
            {
                Collider col = CommonPhysic.GetNearestSurfaceBySphere(
                    creepPoint.worldPosition,
                    1.25f / pointsPerAxis,
                    mask
                );

                creepPoint.active = col != null;

                if (col != null)
                {
                    Vector3 closestPoint = col.ClosestPoint(creepPoint.worldPosition);

                    creepPoint.normal = (creepPoint.worldPosition - closestPoint).normalized;
                    creepPoint.worldPosition = closestPoint + creepPoint.normal * 0.03f;
                }
            }

            #endregion

            #region Check overlap

            List<Vector3Int> toDelete = new List<Vector3Int>();
            foreach (CreepPoint creepPoint in creepPoints)
            {
                foreach (CreepPoint neighbor in creepPoint.GetNeighbors())
                {
                    if (Vector3.Distance(creepPoint.worldPosition, neighbor.worldPosition) <
                        0.25f / pointsPerAxis)
                    {
                        toDelete.Add(neighbor.index);
                        neighbor.cubesAffected.ForEach(c => c.Replace(creepPoint, neighbor));
                    }
                }
            }

            foreach (Vector3Int index in toDelete)
            {
                creepPoints[index.x, index.y, index.z].active = false;
            }

            #endregion

            #region Aveage normals

            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (creepPoint == null || !creepPoint.active)
                    continue;

                Vector3 total = Vector3.zero;
                int count = 0;
                foreach (CreepPoint neighbor in creepPoint.GetNeighbors().Where(c => c != null && creepPoint.active))
                {
                    total += neighbor.normal;
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
                foreach (CreepPoint neighbor in creepPoint.GetNeighbors().Where(c => c != null && creepPoint.active))
                {
                    total += neighbor.worldPosition;
                    count++;
                }

                creepPoint.worldPosition = total / count;
            }

            #endregion

            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (creepPoint == null)
                    continue;

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
        }

        private void UpdatePoints()
        {
            List<CreepPoint> toUpdate = new List<CreepPoint>(),
                toRemove = new List<CreepPoint>(),
                toAdd = new List<CreepPoint>();

            foreach (CreepPoint creepPoint in toUpdateSource)
            {
                List<CreepPoint> spreadToNeighbors = new List<CreepPoint>();

                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        for (int z = -1; z < 2; z++)
                        {
                            Vector3Int index = creepPoint.index;
                            if (index.x + x < 0 || index.x + x >= creepPoints.GetLength(0) ||
                                index.y + y < 0 || index.y + y >= creepPoints.GetLength(1) ||
                                index.z + z < 0 || index.z + z >= creepPoints.GetLength(2))
                                continue;

                            spreadToNeighbors.Add(creepPoints[index.x + x, index.y + y, index.z + z]);
                        }
                    }
                }

                spreadToNeighbors = spreadToNeighbors.Where(cp =>
                    cp != null &&
                    cp.active &&
                    cp.GetSpread() < 1 &&
                    !toUpdate.Contains(cp)).ToList();

                if (spreadToNeighbors.Count == 0)
                {
                    toRemove.Add(creepPoint);
                    continue;
                }

                toUpdate.AddRange(spreadToNeighbors);
            }

            foreach (CreepPoint neighbor in toUpdate)
            {
                neighbor.SetSpread(
                    Mathf.Clamp(
                        neighbor.GetSpread() + spreadPercentagePerSecond *
                        Random.Range(randomSpread.x, randomSpread.y) *
                        Time.deltaTime,
                        0,
                        1
                    )
                );

                if (neighbor.GetSpread() > 0.5f)
                    toAdd.Add(neighbor);
            }

            toUpdateSource.AddRange(toAdd);
            toRemove.ForEach(e => toUpdateSource.Remove(e));
        }

        private IEnumerator UsablePoints()
        {
            List<Vector3Int> checkedPoints = new List<Vector3Int>(),
                toCheck = new List<Vector3Int>();

            toCheck.Add(CommonVariable.MultiDimensionalToList(creepPoints)
                .Where(cp => cp != null && cp.active)
                .OrderBy(cp => Vector3.Distance(cp.worldPosition, checkFromPoint.position)).First().index);

            int offset = 0;
            while (toCheck.Count > 0)
            {
                CreepPoint point = creepPoints[toCheck[0].x, toCheck[0].y, toCheck[0].z];

                checkedPoints.Add(toCheck[0]);
                toCheck.RemoveAt(0);

                if (point == null || !point.active)
                    continue;

                foreach (CreepPoint neighbor in point.GetNeighbors()
                             .Where(cp => cp != null && cp.active && !checkedPoints.Contains(cp.index)))
                {
                    Vector3 pPos = point.worldPosition + point.normal * 0.05f,
                        nPos = neighbor.worldPosition + neighbor.normal * 0.05f;
                    Vector3 dir = nPos - pPos;

                    if (!Physics.Raycast(neighbor.worldPosition, -dir.normalized, dir.magnitude, mask,
                            QueryTriggerInteraction.Ignore) &&
                        !Physics.Raycast(point.worldPosition, dir.normalized, dir.magnitude, mask,
                            QueryTriggerInteraction.Ignore))
                    {
                        if (!checkedPoints.Contains(neighbor.index) && !toCheck.Contains(neighbor.index))
                            toCheck.Add(neighbor.index);

                        point.AddConnected(neighbor);
                        neighbor.AddConnected(point);
                    }
                }

                point.active = point.GetConnectedNeighbors().Length > 0;

                offset++;

                if (offset % 5 == 0)
                    yield return null;
            }

            Debug.Log("Done");
            ready = true;
        }

        #endregion
    }
}