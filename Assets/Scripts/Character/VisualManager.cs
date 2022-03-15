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

        private static readonly int moveY = Animator.StringToHash("MoveY"),
            moveX = Animator.StringToHash("MoveX");

        #endregion

        #region Build In States

        private void OnEnable()
        {
            InputManager.instance.moveEvent.AddListener(OnMoveUpdate);
        }

        private void OnDisable()
        {
            InputManager.instance.moveEvent.RemoveListener(OnMoveUpdate);
        }

        private void Awake()
        {
            animator = nonPlayerVisual.transform.GetChild(0).GetComponent<Animator>();

            foreach (Renderer r in CommonGameObject.GetAllComponentsByRoot<Renderer>(nonPlayerVisual))
                Debug.Log(r == null);

            if(pv.IsMine)
                SetAsPlayer();
            else 
                SetAsNonPlayer();
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
            animator.SetFloat(moveX, input.x);
            animator.SetFloat(moveY, input.y);
        }

        #endregion
    }
}