using UnityEngine;
using TMPro; // Using TextMeshPro for better text quality

public class GameInstructionText : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI instructionText;
    
    [Header("Text Settings")]
    [TextArea(3, 5)]
    public string instructionMessage = "Press E to interact. Activate all buttons to open the door.";
    
    [Header("Display Settings")]
    public float displayDuration = 10f; // How long the text stays on screen
    public bool fadeOut = true; // Whether to fade out or just disappear
    public float fadeOutDuration = 1.5f; // Time to fade out
    
    private void Start()
    {
        // Make sure we have a reference to the text component
        if (instructionText == null)
        {
            instructionText = GetComponent<TextMeshProUGUI>();
            if (instructionText == null)
            {
                Debug.LogError("No TextMeshProUGUI component found!");
                return;
            }
        }
        
        // Set the instruction text
        instructionText.text = instructionMessage;
        
        // Start the auto-hide coroutine if needed
        if (displayDuration > 0)
        {
            StartCoroutine(HideAfterDelay());
        }
    }
    
    private System.Collections.IEnumerator HideAfterDelay()
    {
        // Wait for the display duration
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out or hide immediately
        if (fadeOut)
        {
            float elapsed = 0f;
            Color originalColor = instructionText.color;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                instructionText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            
            // Ensure it's fully invisible
            instructionText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        }
        else
        {
            // Just hide it immediately
            gameObject.SetActive(false);
        }
    }
}