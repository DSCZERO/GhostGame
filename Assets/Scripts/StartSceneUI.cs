using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneUI : MonoBehaviour
{
    void Start()
    {
        // Ensure cursor is visible and unlocked in the title scene
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
        SceneManager.LoadScene("Level_3");
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