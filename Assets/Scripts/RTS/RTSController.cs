#region Packages

using System;
using GameDev.Input;

#endregion

namespace GameDev.RTS
{
    public sealed class RtsController : Controller
    {
        #region Values

        private float mouseScroll;

        #endregion

        #region Build In States

        protected override void Start()
        {
            base.Start();

            InputManager.instance.mouseScrollEvent.AddListener(OnMouseScrollUpdate);
        }

        private void Update()
        {
            if (photonView.IsMine)
                Move();
        }

        #endregion

        #region Internal

        private void Move()
        {
        }

        private void OnMouseScrollUpdate(float input)
        {
            mouseScroll = input;
        }

        #endregion
    }
}