#region Packages

using UnityEngine;

#endregion

namespace GameDev.Input
{
    public abstract class Controller : MonoBehaviour
    {
        #region Values

        protected Transform objTransform;
        protected Vector2 moveDir, rotDir;

        #endregion

        #region Build In States

        protected virtual void Start()
        {
            objTransform = transform;

            InputManager.instance.moveEvent.AddListener(OnMoveAxisUpdate);
            InputManager.instance.rotEvent.AddListener(OnRotAxisUpdate);
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