#region Packages

using UnityEngine;
using UnityEngine.UI;

#endregion

namespace GameDev.UI.FPS
{
    public class Healthbar : MonoBehaviour
    {
        #region Values

        [SerializeField] private Slider slider;

        #endregion

        #region In

        public void Set(float input)
        {
            slider.value = input;
        }

        #endregion
    }
}
