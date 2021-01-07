using PlayerControls;
using UnityEngine;
using UnityEngine.UI;

public class NoStaminaIndicatorController : MonoBehaviour
{
    private Color _baseColor;
    private float _alpha;

    public StaminaStats staminaStats;
    public PlayerMovementStats playerMovementStats;
    public Image noStaminaIndicatorImage;
    
    
    private void Start()
    {
        _baseColor = noStaminaIndicatorImage.color;
    }

    private void Update()
    {
        if (staminaStats.CurrentValue < 0 || playerMovementStats.movementState == MovementState.Tired)
        {
            var lerp = Mathf.PingPong(Time.time, 1) / 1;
             _alpha = Mathf.Lerp(0, 1, Mathf.SmoothStep(0, 1, lerp));
             _baseColor.a = _alpha;
             noStaminaIndicatorImage.color = _baseColor;
        }
        else if(!Mathf.Approximately(noStaminaIndicatorImage.color.a, 0))
        {
            _baseColor.a = 0;
            noStaminaIndicatorImage.color = _baseColor;
        }
    }
}
