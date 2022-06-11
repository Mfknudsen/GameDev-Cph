#region Packages

using TMPro;
using UnityEngine;

#endregion

namespace GameDev.UI.FPS
{
    public class VictoryScreenUI : MonoBehaviour
    {
        #region Values

        public static VictoryScreenUI instance;
        
        [SerializeField] private TextMeshProUGUI text;

        #endregion

        #region Build In States

        private void Start()
        {
            transform.GetChild(0).gameObject.SetActive(false);

            if(instance != null)
                Destroy(instance);
            
            instance = this;
        }

        #endregion

        #region In

        public void DisplayMessage(bool result)
        {
            transform.GetChild(0).gameObject.SetActive(true);

            text.text = result
                ? "Congratulation!\nYou defeated the enemy team"
                : "Defeat!\nYou failed to achieve victory";
        }

        #endregion
    }
}