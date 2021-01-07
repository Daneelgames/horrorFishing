using UnityEngine;

namespace PlayerControls
{
    [CreateAssetMenu(menuName = "Player movement stats", order = 0)]
    public class PlayerMovementStats : ScriptableObject
    {
        public MovementState movementState;
        public bool isRunning;
        public bool keepsRunningWithZeroStamina;
        public float currentMoveSpeed;
        
        public float tiredSpeed = 2.5f;
        public float baseMoveSpeed = 10;
        public float runSpeedBonusCurrent = 5;
        public float runSpeedBonus = 5;
        public float runSpeedBonusBoosted = 10;
        public float dashSpeed = 20;
        public float dashSpeedNoStamina = 10;
        
        public float dashTime = 0.75f;
        public float dashTimeNoStamina = 0.55f;
        public float dashCooldown = 0.3f;
    }
}