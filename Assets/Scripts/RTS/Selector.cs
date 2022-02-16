#region Packages

using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using GameDev.Buildings;
using GameDev.Input;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace GameDev.RTS
{
    public class Selector : MonoBehaviour
    {
        #region Values

        [SerializeField] private GameObject test;

        //Selecting
        [SerializeField] private LayerMask selectMask;
        private bool selecting;
        private Camera cam;
        private Vector2 startPoint, endPoint;
        private List<IUnit> selectedUnits = new List<IUnit>();

        //Placing Buildings
        [SerializeField] private LayerMask placingMask;
        private bool placing;
        private Building placingObject;

        #endregion

        #region Build In States

        private void Start()
        {
            InputManager.instance.shootEvent.AddListener(OnShootUpdate);
            cam ??= GetComponent<Camera>();

            ToPlaceBuilding(Instantiate(test).GetComponent<Building>());
        }

        private void Update()
        {
            if (!placing)
                return;

            RaycastHit? hit = RayHit(Mathf.Infinity, placingMask);
            if (hit.HasValue)
                placingObject.transform.position = hit.Value.point;
        }

        #endregion

        #region Getters

        public IUnit[] GetSelectedUnits()
        {
            return selectedUnits.ToArray();
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
                    selectedUnits.Clear();

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

            foreach (MonoBehaviour behaviour in FindObjectsOfType<MonoBehaviour>().Where(m => m.IsType<IUnit>()))
            {
                if (!bounds.Contains(cam.WorldToScreenPoint(behaviour.transform.position))) return;

                // ReSharper disable once SuspiciousTypeConversion.Global
                IUnit unit = behaviour as IUnit;
                unit?.Select(this);
                selectedUnits.Add(unit);
            }
        }

        private void SingleSelect()
        {
            RayHit(2000, selectMask)?.transform.GetComponent<IUnit>()?.Select(this);
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