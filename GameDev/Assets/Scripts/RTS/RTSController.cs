#region Packages

using GameDev.Input;

#endregion

namespace GameDev.RTS
{
    public sealed class RTSController : Controller
    {
        #region Values

        private float mouseScroll;

        #endregion

        #region Build In States

        private void Start()
        {
            InputManager.Instance.mouseScrollEvent.AddListener(OnMouseScrollUpdate);
        }

        #endregion

        #region Internal

        private void OnMouseScrollUpdate(float input)
        {
            mouseScroll = input;
        }

        #endregion
    }
}