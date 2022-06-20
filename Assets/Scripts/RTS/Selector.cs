#region Packages

using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using GameDev.Buildings;
using GameDev.Common;
using GameDev.Input;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace GameDev.RTS
{
    public class Selector : MonoBehaviour
    {
        #region Values

        //Selecting
        [SerializeField] private LayerMask selectMask;
        private bool selecting;
        private Camera cam;
        private Vector2 startPoint, endPoint;
        private readonly List<ISelectable> selected = new List<ISelectable>();

        //Placing Buildings
        [SerializeField] private LayerMask placingMask;
        private bool placing;
        private Building placingObject;

        #endregion

        #region Build In States

        private void OnEnable()
        {
            InputManager.instance.shootEvent.AddListener(OnShootUpdate);
        }

        private void OnDisable()
        {
            InputManager.instance.shootEvent.RemoveListener(OnShootUpdate);
        }

        private void Start()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            if (!placing)
                return;

            RaycastHit? hit = RayHit(Mathf.Infinity, placingMask);
            if (hit.HasValue)
            {
                placingObject.gameObject.SetActive(true);

                if (placingObject is RestrictedBuilding restrictedBuilding &&
                    CommonPhysic.HitComponent<BuildingPlacement>(hit.Value) is
                        { } buildingPlacement)
                {
                    if (buildingPlacement.GetTypeAllowed().Equals(restrictedBuilding.GetBuildingType()))
                    {
                        placingObject.transform.position = restrictedBuilding.transform.position;
                        restrictedBuilding.SetCanPlace(true);
                    }
                    else
                    {
                        placingObject.transform.position = hit.Value.point;
                        restrictedBuilding.SetCanPlace(false);
                    }
                }
                else
                    placingObject.transform.position = hit.Value.point;
            }
            else
                placingObject.gameObject.SetActive(false);
        }

        #endregion

        #region Getters

        public ISelectable[] GetSelectedUnits()
        {
            return selected.ToArray();
        }

        #endregion

        #region In

        public void ToPlaceBuilding(Building buildingObject)
        {
            if (buildingObject != null)
            {
                GameObject o = buildingObject.gameObject;
                o.name = o.name.Replace("(Clone)", "");
            }

            placing = buildingObject != null;
            placingObject = buildingObject;
        }

        public void AddSelectedToList(ISelectable selectable)
        {
            if (!selected.Contains(selectable))
                selected.Add(selectable);
        }

        public void RemoveSelectedFromList(ISelectable selectable)
        {
            selected.Remove(selectable);
        }

        #endregion

        #region Internal

        private void OnShootUpdate()
        {
            if (placing && placingObject != null)
            {
                if (placingObject.CanBePlaced())
                {
                    placingObject.Place();
                    ToPlaceBuilding(null);
                }
            }
            else
            {
                selecting = !selecting;

                if (selecting)
                    startPoint = Mouse.current.position.ReadValue();
                else
                {
                    selected.Clear();

                    endPoint = Mouse.current.position.ReadValue();

                    if ((startPoint - endPoint).magnitude > 40)
                        BoxSelect();
                    else
                        SingleSelect();
                }
            }
        }

        private void BoxSelect()
        {
            Rect selectionBox = new Rect();
            //Calculate X
            if (endPoint.x < startPoint.x)
            {
                selectionBox.xMin = endPoint.x;
                selectionBox.xMax = startPoint.x;
            }
            else
            {
                selectionBox.xMin = startPoint.x;
                selectionBox.xMax = endPoint.x;
            }

            //Calculate Y
            if (endPoint.y < startPoint.y)
            {
                selectionBox.yMin = endPoint.y;
                selectionBox.yMax = startPoint.y;
            }
            else
            {
                selectionBox.yMin = startPoint.y;
                selectionBox.yMax = endPoint.y;
            }

            Bounds bounds = new Bounds(selectionBox.min + selectionBox.max / 2, selectionBox.max);

            foreach (MonoBehaviour behaviour in FindObjectsOfType<MonoBehaviour>().Where(m => m.IsType<ISelectable>()))
            {
                if (!bounds.Contains(cam.WorldToScreenPoint(behaviour.transform.position))) return;

                // ReSharper disable once SuspiciousTypeConversion.Global
                ISelectable unit = behaviour as ISelectable;
                unit?.OnSelect(this);
                selected.Add(unit);
            }
        }

        private void SingleSelect()
        {
            RayHit(2000, selectMask)?.transform.GetComponent<ISelectable>()?.OnSelect(this);
        }

        private RaycastHit? RayHit(float distance, LayerMask mask)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, distance, mask))
                return hit;

            return null;
        }

        #endregion
    }
}