using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneUI : MonoBehaviour
{
    void Start()
    {
        // Ensure cursor is visible and unlocked in the start scene
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenScene()
    {
        SceneManager.LoadScene("TutorialScene");
    }

    public void OpenSceneOne()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void OpenSceneTwo()
    {
        SceneManager.LoadScene("Level 2");
    }

    public void OpenSceneThree()
    {
        SceneManager.LoadScene("Level 3");
    }

    public void OpenSceneFour()
    {
        SceneManager.LoadScene("StartLevel4");
    }

    public void OpenLVL4()
    {
        SceneManager.LoadScene("Level 4");
    }

    public void ExitGame()
    {
        Application.Quit();
        
        // Provide feedback when testing in Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}