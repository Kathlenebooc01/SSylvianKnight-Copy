using UnityEngine;
using UnityEngine.UI;

public class HealthManagerUI : MonoBehaviour
{
    [Header("Player Reference")]
    public PlayerMovement player;    // Drag your Player object here

    [Header("Mask Settings")]
    public Image[] maskImages;      // Drag your 5 Mask Images here
    public Sprite fullMask;         // Your standard mask sprite
    public Sprite emptyMask;        // A broken/grayed version (Optional)

    private int healthPerMask = 20; // 100 max health / 5 masks

    void Update()
    {
        if (player == null) return;

        // Get the current health from the PlayerMovement script
        int currentHealth = player.GetCurrentHealth();

        for (int i = 0; i < maskImages.Length; i++)
        {
            // Logic: Is health high enough to fill this specific mask?
            // Mask 0 needs 20HP, Mask 1 needs 40HP, etc.
            if (currentHealth >= (i + 1) * healthPerMask)
            {
                maskImages[i].sprite = fullMask;
                maskImages[i].enabled = true;
            }
            else
            {
                // If health is too low, show empty sprite or just hide it
                if (emptyMask != null)
                {
                    maskImages[i].sprite = emptyMask;
                }
                else
                {
                    maskImages[i].enabled = false;
                }
            }
        }
    }
}