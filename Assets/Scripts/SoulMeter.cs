using UnityEngine;
using UnityEngine.UI;

public class SoulMeter : MonoBehaviour
{
    public PlayerMovement player; // Drag your Player object here
    public Image soulOrbImage;    // Drag your UI Circle Image here

    void Update()
    {
        if (player != null && soulOrbImage != null)
        {
            // Calculate fill amount (0.0 to 1.0)
            float fillAmount = (float)player.currentSoul / player.maxSoul;
            soulOrbImage.fillAmount = fillAmount;
        }
    }
}