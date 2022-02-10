#region Packages

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
            InputManager.instance.mouseScrollEvent.AddListener(OnMouseScrollUpdate);
        }

        #endregion

        #region Internal

        protected override void UpdateOwned()
        {
            Move();
        }

        protected override void UpdateUnowned()
        {
        }

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