using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneUI : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OpenScene()
    {
        // Unlock cursor before loading the StartScene
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("StartScene");
    }
}