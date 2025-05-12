using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Manages level completion UI and scene transition
/// This script dynamically creates UI elements when triggered
/// </summary>
public class LevelCompleteManager : MonoBehaviour
{
    [Header("Text Settings")]
    [TextArea(3, 5)]
    public string completionMessage = "Congratulations! You will proceed to level 2 in 5 seconds.";
    public Font textFont; // Optional, default font will be used if not set
    public int fontSize = 36;
    public Color textColor = Color.yellow;
    
    [Header("Level Settings")]
    public string nextLevelName = "Level 2"; // Name of the next scene to load
    public float waitTimeBeforeLoad = 5f; // Seconds to wait before loading next level
    
    [Header("Trigger Settings")]
    public string playerTag = "Player"; // Tag of the player object
    
    // Private variables
    private Canvas uiCanvas;
    private TextMeshProUGUI completionText;
    private bool isTriggered = false;
    
    private void Awake()
    {
        // Don't create UI on initialization, only when triggered
    }
    
    /// <summary>
    /// Detects when player enters the trigger area
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player
        if (other.CompareTag(playerTag) && !isTriggered)
        {
            // Make sure player is not in ghost mode
            GhostMode ghostMode = other.GetComponentInParent<GhostMode>();
            if (ghostMode != null && ghostMode.IsInGhostMode) return;
            
            TriggerLevelComplete();
        }
    }
    
    /// <summary>
    /// Triggers level completion sequence
    /// Can be called from other scripts if needed
    /// </summary>
    public void TriggerLevelComplete()
    {
        if (isTriggered) return; // Prevent multiple triggers
        isTriggered = true;
        
        // Create UI elements
        CreateCompletionUI();
        
        // Start countdown to next level
        StartCoroutine(LoadNextLevelAfterDelay());
    }
    
    /// <summary>
    /// Dynamically creates UI canvas and text elements
    /// </summary>
    private void CreateCompletionUI()
    {
        // 1. Create Canvas
        GameObject canvasObj = new GameObject("CompletionCanvas");
        uiCanvas = canvasObj.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Add CanvasScaler to handle different screen resolutions
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster (required for UI interaction, though not needed here)
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 2. Create text object
        GameObject textObj = new GameObject("CompletionText");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        // Using TextMeshPro for better text quality
        completionText = textObj.AddComponent<TextMeshProUGUI>();
        completionText.text = completionMessage;
        completionText.fontSize = fontSize;
        completionText.color = textColor;
        completionText.alignment = TextAlignmentOptions.Center;
        
        // If you prefer standard Unity Text instead of TextMeshPro,
        // comment the above lines and uncomment these:
        /*
        Text completionText = textObj.AddComponent<Text>();
        completionText.text = completionMessage;
        completionText.fontSize = fontSize;
        completionText.color = textColor;
        completionText.alignment = TextAnchor.MiddleCenter;
        if (textFont != null)
            completionText.font = textFont;
        */
        
        // 3. Position the text (centered on screen)
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(800, 200); // Width and height - adjust as needed
    }
    
    /// <summary>
    /// Coroutine that handles countdown and scene transition
    /// Updates text with remaining seconds before loading next level
    /// </summary>
    private IEnumerator LoadNextLevelAfterDelay()
    {
        float remainingTime = waitTimeBeforeLoad;
        
        // Update countdown text
        while (remainingTime > 0)
        {
            // Update text with current countdown
            if (completionText != null)
            {
                completionText.text = $"Congratulations! You will proceed to {nextLevelName} in {Mathf.CeilToInt(remainingTime)} seconds.";            }
            
            yield return new WaitForSeconds(0.1f); // Update more frequently for smoother countdown
            remainingTime -= 0.1f;
        }
        
        // Load the next level
        SceneManager.LoadScene(nextLevelName);
    }
}