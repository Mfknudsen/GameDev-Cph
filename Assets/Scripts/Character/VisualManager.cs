#region Packages

using GameDev.Common;
using GameDev.Input;
using Photon.Pun;
using UnityEngine;

#endregion

namespace GameDev.Character
{
    public class VisualManager : MonoBehaviour
    {
        #region Values

        [SerializeField] private PhotonView pv;
        [SerializeField] private GameObject playerVisual, nonPlayerVisual;

        private Animator animator;

        private Vector3 dir = Vector3.zero;

        private static readonly int moveY = Animator.StringToHash("MoveY"),
            moveX = Animator.StringToHash("MoveX");

        #endregion

        #region Build In States

        private void OnEnable()
        {
            if (pv.IsMine)
                InputManager.instance.moveEvent.AddListener(OnMoveUpdate);
        }

        private void OnDisable()
        {
            if (pv.IsMine)
                InputManager.instance.moveEvent.RemoveListener(OnMoveUpdate);
        }

        private void Awake()
        {
            animator = nonPlayerVisual.transform.GetChild(0).GetComponent<Animator>();

            if (pv.IsMine)
                SetAsPlayer();
            else
                SetAsNonPlayer();
        }

        private void Update()
        {
            if (animator == null || !pv.IsMine) return;

            animator.SetFloat(moveX, dir.x * 2, 0.1f, Time.deltaTime);
            animator.SetFloat(moveY, dir.y * 2, 0.1f, Time.deltaTime);
        }

        #endregion

        #region In

        public void SetAsNonPlayer()
        {
            playerVisual.SetActive(false);
            foreach (Renderer meshRenderer in
                     CommonGameObject.GetAllComponentsByRoot<Renderer>(nonPlayerVisual))
                meshRenderer.enabled = true;
        }

        public void SetAsPlayer()
        {
            playerVisual.SetActive(true);
            foreach (Renderer meshRenderer in
                     CommonGameObject.GetAllComponentsByRoot<Renderer>(nonPlayerVisual))
                meshRenderer.enabled = false;
        }

        #endregion

        #region Internal

        private void OnMoveUpdate(Vector2 input)
        {
            dir = input;
        }

        #endregion
    }
}