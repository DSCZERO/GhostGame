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

    public void OpenSceneTwo()
    {
        SceneManager.LoadScene("Level2");
    }

    public void OpenSceneThree()
    {
        SceneManager.LoadScene("Level3");
    }

    public void OpenSceneFour()
    {
        // SceneManager.LoadScene("Level4");
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