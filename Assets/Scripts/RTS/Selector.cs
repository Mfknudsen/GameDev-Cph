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
        
        //Selecting
        [Header("Selecting")] [SerializeField] private GameObject uiPrefab;
        [SerializeField] private LayerMask selectMask;
        [SerializeField] private float minBoxDistance;
        private bool selecting;
        private Camera cam;
        private Vector2 startPoint, endPoint;
        private List<IUnit> selectedUnits = new List<IUnit>();
        private RectTransform uiRectTransform;

        //Placing Buildings
        [Space] [Header("Building")] [SerializeField]
        private LayerMask placingMask;

        private bool placing;
        private Building placingObject;

        #endregion

        #region Build In States

        private void Start()
        {
            InputManager.instance.shootEvent.AddListener(OnShootUpdate);
            cam ??= GetComponent<Camera>();
        }

        private void Update()
        {
            if (selecting && uiRectTransform != null)
            {
                Vector2 size = startPoint - Mouse.current.position.ReadValue(),
                    center = startPoint - size / 2;

                size.x = Mathf.Abs(size.x) / (Screen.width / 100f) / 100 * 1920;
                size.y = Mathf.Abs(size.y) / (Screen.height / 100f) / 100 * 1080;

                uiRectTransform.gameObject.SetActive(size.magnitude >= minBoxDistance);

                uiRectTransform.position = center;
                uiRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                uiRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            }
            else if (placing)
            {
                RaycastHit? hit = RayHit(Mathf.Infinity, placingMask);
                if (hit.HasValue)
                    placingObject.transform.position = hit.Value.point;
            }
        }

        private void OnEnable()
        {
            uiRectTransform = Instantiate(uiPrefab, GameObject.Find("Canvas").transform).transform as RectTransform;

            if (uiRectTransform != null) uiRectTransform.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (uiRectTransform != null)
                Destroy(uiRectTransform.gameObject);
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
                {
                    startPoint = Mouse.current.position.ReadValue();
                    endPoint = Vector2.zero;
                }
                else
                {
                    uiRectTransform.gameObject.SetActive(false);
                    endPoint = Mouse.current.position.ReadValue();
                    selectedUnits.Clear();

                    if ((startPoint - endPoint).magnitude >= minBoxDistance)
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