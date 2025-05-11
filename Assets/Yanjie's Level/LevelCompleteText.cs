using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelCompleteText : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI completionText;
    
    [Header("Text Settings")]
    [TextArea(3, 5)]
    public string completionMessage = "Congratulations! You will proceed to level 2 in 5 seconds.";
    
    [Header("Level Settings")]
    public string nextLevelName = "Level 2";
    public float waitTimeBeforeLoad = 5f;
    
    [Header("Trigger Settings")]
    public bool useCollisionTrigger = true;
    public string playerTag = "Player";
    
    private bool isTriggered = false;
    
    private void Start()
    {
        // Make sure we have a reference to the text component
        if (completionText == null)
        {
            completionText = GetComponent<TextMeshProUGUI>();
            if (completionText == null)
            {
                Debug.LogError("No TextMeshProUGUI component found!");
                return;
            }
        }
        
        // Initially hide the text
        completionText.gameObject.SetActive(false);
    }
    
    // Can be called from another script when the level is completed
    public void TriggerLevelComplete()
    {
        if (isTriggered) return;
        isTriggered = true;
        
        // Show the completion text
        completionText.text = completionMessage;
        completionText.gameObject.SetActive(true);
        
        // Start countdown to next level
        StartCoroutine(LoadNextLevelAfterDelay());
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Only respond to trigger if set to use collision-based triggering
        if (!useCollisionTrigger) return;
        
        // Check if the player entered the trigger zone
        if (other.CompareTag(playerTag) && !isTriggered)
        {
            // Make sure we're not in ghost mode (only physical body should trigger this)
            GhostMode ghostMode = other.GetComponentInParent<GhostMode>();
            if (ghostMode != null && ghostMode.IsInGhostMode) return;
            
            TriggerLevelComplete();
        }
    }
    
    private System.Collections.IEnumerator LoadNextLevelAfterDelay()
    {
        float remainingTime = waitTimeBeforeLoad;
        
        // Update the text each second with remaining time
        while (remainingTime > 0)
        {
            // Update the message with current countdown
            completionText.text = $"Congratulations! You will proceed to level 2 in {Mathf.CeilToInt(remainingTime)} seconds.";
            
            yield return new WaitForSeconds(0.1f); // Update more frequently for smoother display
            remainingTime -= 0.1f;
        }
        
        // Load the next level
        SceneManager.LoadScene(nextLevelName);
    }
}