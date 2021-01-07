using System;
using UnityEngine;

namespace PlayerControls
{
    public class StaminaController : MonoBehaviour
    {
        public StaminaStats staminaStats;
        public PlayerMovementStats movementStats;
        
        private void Update()
        {
            float staminaRegen;
            float staminaBuffScaler = 1;
            if (PlayerMovement.instance.hc.rustAndPoisonActive) staminaBuffScaler = 0.5f;
            
            switch (movementStats.movementState)
            {
                case MovementState.Idle:
                    staminaRegen = staminaStats.idleRegen;
                    break;
                case MovementState.Walking:
                    staminaRegen = movementStats.isRunning
                        ? staminaStats.runRegenCurrent * staminaBuffScaler 
                        : staminaStats.walkRegen;
                    break;
                
                case MovementState.Dashing:
                case MovementState.DashingNoStamina:
                    staminaRegen = 0;
                    break;
                    
                case MovementState.Tired:
                    staminaRegen = movementStats.keepsRunningWithZeroStamina ? 0 : staminaStats.tiredRegen;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(movementStats.movementState));
            }
            
            staminaStats.CurrentValue += staminaRegen * Time.deltaTime;
        }
    }
}
