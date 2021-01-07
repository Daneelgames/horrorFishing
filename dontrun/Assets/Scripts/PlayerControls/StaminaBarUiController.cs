using UnityEngine;
using UnityEngine.UI;

namespace PlayerControls
{
    public class StaminaBarUiController : MonoBehaviour
    {
        public StaminaStats staminaStats;
        public Image barImage;

        private void Update()
        {
            barImage.fillAmount = staminaStats.CurrentValue / staminaStats.MaxValue;
        }
    }
}