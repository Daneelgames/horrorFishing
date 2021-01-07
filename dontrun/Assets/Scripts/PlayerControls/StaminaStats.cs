using UnityEngine;

namespace PlayerControls
{
    [CreateAssetMenu(menuName = "Stamina Stats", order = 0)]
    public class StaminaStats : ScriptableObject
    {
        public float idleRegen = 20f;
        public float walkRegen = 12f;
        public float runRegenCurrent = -5f;
        public float runRegen = -5f;
        public float boostedRunRegen = -10f;
        public float tiredRegen = 0;
        public float dashCostCurrent = 30;
        public float dashCost = 60;
        public float boostedDashCost = 60;
        
        [SerializeField] 
        private float currentValue = 0;
        public float CurrentValue
        {
            get => currentValue;
            set
            {
                if (value > maxValue)
                    currentValue = maxValue;
                else if (value < minValue)
                    currentValue = minValue;
                else
                    currentValue = value;
            }
        }

        [SerializeField] 
        private float maxValue = 200;
        public float MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = value;
                if (CurrentValue > maxValue)
                    CurrentValue = maxValue;
            }
        }
        
        [SerializeField] 
        [Header("Min possible value when action reduce stamina bellow 0.")]
        [Range(-1000, 0)]
        private float minValue = - 50;
        public float MinValue
        {
            get => minValue;
            set
            {
                minValue = value;
                if (CurrentValue < minValue)
                    CurrentValue = minValue;
            }
        }
  }
}