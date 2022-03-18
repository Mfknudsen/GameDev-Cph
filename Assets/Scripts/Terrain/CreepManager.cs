#region Packages

using System.Collections.Generic;
using System.Linq;
using GameDev.Common;
using UnityEditor;
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
        [SerializeField] private Material creepMaterial;
        [SerializeField] private float distanceOfSurface = 0.2f;
        [SerializeField] private AnimationCurve riseCurve;

        private CreepPoint[,,] creepPoints;

        private List<CreepPoint> toUpdateSource = new List<CreepPoint>();

        #region Marching Cubes

        private MeshFilter meshFilter;

        #endregion

        #endregion

        #region Build In States

        private void Start()
        {
            GeneratePoints();
        }

        private void Update()
        {
            UpdatePoints();

            if (meshFilter == null)
            {
                GameObject obj = new GameObject("Mesh");
                meshFilter = obj.AddComponent<MeshFilter>();
                obj.AddComponent<MeshRenderer>().material = creepMaterial;
            }

            int vertIndex = 0;
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            foreach (CreepPoint creepPoint in creepPoints)
            {
                creepPoint.vertIndex = -1;

                if (!creepPoint.active || creepPoint.spread == 0)
                    continue;

                verts.Add(creepPoint.worldPosition +
                          creepPoint.normal * (riseCurve.Evaluate(creepPoint.spread) * distanceOfSurface));
                creepPoint.vertIndex = vertIndex;
                vertIndex++;
            }

            foreach (CreepPoint creepPoint in creepPoints)
            {
                if (!creepPoint.active || creepPoint.GetNeighbors().Length < 3)
                    continue;

                if (!CanDraw(creepPoint.GetNeighbors(), creepPoint.vertIndex))
                    continue;

                CreepPoint[] neighbors = creepPoint.GetNeighbors();

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[2].vertIndex, neighbors[1].vertIndex }))
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[2].vertIndex);
                    tris.Add(neighbors[1].vertIndex);
                }

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[0].vertIndex, neighbors[2].vertIndex }))
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[0].vertIndex);
                    tris.Add(neighbors[2].vertIndex);
                }

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[3].vertIndex, neighbors[4].vertIndex }))
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[3].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                }

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[5].vertIndex, neighbors[4].vertIndex }) &&
                    neighbors[3].vertIndex.Equals(-1))
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[5].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                }

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[6].vertIndex, neighbors[4].vertIndex }) &&
                    (neighbors[3].vertIndex < 0 && neighbors[5].vertIndex < 0))
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                }

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[0].vertIndex, neighbors[4].vertIndex }))
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                    tris.Add(neighbors[0].vertIndex);
                }

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[3].vertIndex, neighbors[5].vertIndex }))
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[5].vertIndex);
                    tris.Add(neighbors[3].vertIndex);
                }

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[1].vertIndex, neighbors[5].vertIndex }))
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[1].vertIndex);
                    tris.Add(neighbors[5].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[3].vertIndex, neighbors[4].vertIndex, neighbors[5].vertIndex }))
                {
                    tris.Add(neighbors[5].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                    tris.Add(neighbors[3].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[4].vertIndex, neighbors[6].vertIndex, neighbors[5].vertIndex }))
                {
                    tris.Add(neighbors[5].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[4].vertIndex, neighbors[6].vertIndex, neighbors[2].vertIndex }))
                {
                    tris.Add(neighbors[2].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[4].vertIndex, neighbors[2].vertIndex, neighbors[0].vertIndex }))
                {
                    tris.Add(neighbors[0].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                    tris.Add(neighbors[2].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[5].vertIndex, neighbors[6].vertIndex }))
                {
                    tris.Add(neighbors[1].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                    tris.Add(neighbors[5].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[2].vertIndex, neighbors[6].vertIndex }))
                {
                    tris.Add(neighbors[1].vertIndex);
                    tris.Add(neighbors[2].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                }

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[2].vertIndex, neighbors[4].vertIndex }) &&
                    neighbors[0].vertIndex < 0)
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                    tris.Add(neighbors[2].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[0].vertIndex, neighbors[6].vertIndex }) &&
                    neighbors[2].vertIndex < 0)
                {
                    tris.Add(neighbors[1].vertIndex);
                    tris.Add(neighbors[0].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[0].vertIndex, neighbors[4].vertIndex, neighbors[6].vertIndex }))
                {
                    tris.Add(neighbors[0].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                }

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[0].vertIndex, neighbors[1].vertIndex }))
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[0].vertIndex);
                    tris.Add(neighbors[1].vertIndex);
                }

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[1].vertIndex, neighbors[3].vertIndex }))
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[1].vertIndex);
                    tris.Add(neighbors[3].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[3].vertIndex, neighbors[6].vertIndex }) &&
                    neighbors[5].vertIndex < 0)
                {
                    tris.Add(neighbors[1].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                    tris.Add(neighbors[3].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[3].vertIndex, neighbors[4].vertIndex, neighbors[6].vertIndex }) &&
                    neighbors[5].vertIndex < 0)
                {
                    tris.Add(neighbors[3].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                }

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[3].vertIndex, neighbors[6].vertIndex }) &&
                    neighbors[1].vertIndex < 0)
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                    tris.Add(neighbors[3].vertIndex);
                }

                if (ValidTriangle(new[] { creepPoint.vertIndex, neighbors[0].vertIndex, neighbors[6].vertIndex }) &&
                    neighbors[1].vertIndex < 0)
                {
                    tris.Add(creepPoint.vertIndex);
                    tris.Add(neighbors[0].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[0].vertIndex, neighbors[4].vertIndex }) &&
                    neighbors[3].vertIndex < 0 && creepPoint.vertIndex < 0)
                {
                    tris.Add(neighbors[0].vertIndex);
                    tris.Add(neighbors[1].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[5].vertIndex, neighbors[1].vertIndex, neighbors[4].vertIndex }) &&
                    neighbors[3].vertIndex < 0 && creepPoint.vertIndex < 0)
                {
                    tris.Add(neighbors[1].vertIndex);
                    tris.Add(neighbors[5].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[0].vertIndex, neighbors[1].vertIndex, neighbors[2].vertIndex }) &&
                    creepPoint.vertIndex < 0)
                {
                    tris.Add(neighbors[0].vertIndex);
                    tris.Add(neighbors[2].vertIndex);
                    tris.Add(neighbors[1].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[2].vertIndex, neighbors[4].vertIndex }) &&
                    neighbors[0].vertIndex < 0)
                {
                    tris.Add(neighbors[1].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                    tris.Add(neighbors[2].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[0].vertIndex, neighbors[1].vertIndex, neighbors[3].vertIndex }) &&
                    creepPoint.vertIndex < 0)
                {
                    tris.Add(neighbors[0].vertIndex);
                    tris.Add(neighbors[1].vertIndex);
                    tris.Add(neighbors[3].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[5].vertIndex, neighbors[1].vertIndex, neighbors[3].vertIndex }) &&
                    creepPoint.vertIndex < 0)
                {
                    tris.Add(neighbors[1].vertIndex);
                    tris.Add(neighbors[5].vertIndex);
                    tris.Add(neighbors[3].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[0].vertIndex, neighbors[4].vertIndex, neighbors[3].vertIndex }) &&
                    creepPoint.vertIndex < 0)
                {
                    tris.Add(neighbors[0].vertIndex);
                    tris.Add(neighbors[3].vertIndex);
                    tris.Add(neighbors[4].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[3].vertIndex, neighbors[5].vertIndex, neighbors[6].vertIndex }) &&
                    neighbors[4].vertIndex < 0)
                {
                    tris.Add(neighbors[3].vertIndex);
                    tris.Add(neighbors[5].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[0].vertIndex, neighbors[2].vertIndex, neighbors[6].vertIndex }) &&
                    neighbors[4].vertIndex < 0)
                {
                    tris.Add(neighbors[0].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                    tris.Add(neighbors[2].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[0].vertIndex, neighbors[3].vertIndex, neighbors[6].vertIndex }) &&
                    neighbors[4].vertIndex < 0)
                {
                    tris.Add(neighbors[0].vertIndex);
                    tris.Add(neighbors[3].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[2].vertIndex, neighbors[3].vertIndex, neighbors[6].vertIndex }) &&
                    neighbors[4].vertIndex < 0 && neighbors[0].vertIndex < 0)
                {
                    tris.Add(neighbors[2].vertIndex);
                    tris.Add(neighbors[3].vertIndex);
                    tris.Add(neighbors[6].vertIndex);
                }

                if (ValidTriangle(new[] { neighbors[1].vertIndex, neighbors[2].vertIndex, neighbors[3].vertIndex }) &&
                    creepPoint.vertIndex < 0 && neighbors[0].vertIndex < 0)
                {
                    tris.Add(neighbors[1].vertIndex);
                    tris.Add(neighbors[3].vertIndex);
                    tris.Add(neighbors[2].vertIndex);
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "Creep Mesh";
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)fieldSize / 2, fieldSize);

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

                continue;
                for (int i = 0; i < creepPoint.GetNeighbors().Length; i++)
                    drawString(i.ToString(), creepPoint.GetNeighbors()[i].worldPosition);
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

                if (index.x == creepPoints.GetLength(0) - 1 ||
                    index.y == creepPoints.GetLength(1) - 1 ||
                    index.z == creepPoints.GetLength(2) - 1)
                    continue;

                List<CreepPoint> toSet = new List<CreepPoint>();

                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int z = 0; z < 2; z++)
                        {
                            Vector3Int toLook = index + new Vector3Int(x, y, z);

                            if (index.Equals(toLook))
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
                    0.5f / pointsPerAxis,
                    mask
                );

                creepPoint.active = col != null;

                if (col != null)
                {
                    Vector3 closestPoint = col.ClosestPoint(creepPoint.worldPosition);

                    creepPoint.normal = (creepPoint.worldPosition - closestPoint).normalized;
                    creepPoint.worldPosition = closestPoint;
                }
            }

            #endregion

            List<CreepPoint> temp = new List<CreepPoint>();
            foreach (CreepPoint creepPoint in creepPoints)
                temp.Add(creepPoint);

            toUpdateSource.Add(temp.Where(cp => cp.active)
                .OrderBy(cp => Vector3.Distance(cp.worldPosition, checkFromPoint.position))
                .First());
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
                    cp.active &&
                    cp.spread < 1 &&
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

        private bool ValidTriangle(int[] vertValues)
        {
            foreach (int vertValue in vertValues)
            {
                if (vertValue < 0)
                    return false;
            }

            return true;
        }

        private bool CanDraw(CreepPoint[] points, int origin)
        {
            int i = origin < 0 ? 0 : 1;

            foreach (CreepPoint creepPoint in points)
            {
                if (creepPoint.spread > 0)
                {
                    i++;

                    if (i.Equals(3)) return true;
                }
            }

            return false;
        }

        #endregion

        static void drawString(string text, Vector3 worldPos, Color? colour = null)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 30;
            Handles.BeginGUI();
            if (colour.HasValue) GUI.color = colour.Value;
            SceneView view = SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y),
                text, style);
            Handles.EndGUI();
        }
    }
}