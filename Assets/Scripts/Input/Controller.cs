#region Packages

using UnityEngine;

#endregion

namespace GameDev.Input
{
    public abstract class Controller : MonoBehaviour
    {
        #region Values

        protected Vector2 moveDir, rotDir;

        #endregion

        #region Build In States

        protected Controller()
        {
            InputManager.Instance.moveEvent.AddListener(OnMoveAxisUpdate);
            InputManager.Instance.rotEvent.AddListener(OnRotAxisUpdate);
        }

        #endregion

        #region Internal

        private void OnMoveAxisUpdate(Vector2 input)
        {
            moveDir = input;
        }

        private void OnRotAxisUpdate(Vector2 input)
        {
            rotDir = input;
        }

        #endregion
    }
}