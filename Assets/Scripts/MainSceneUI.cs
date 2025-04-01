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
        if (Input.GetKeyDown(KeyCode.Escape)) {
            OpenScene();
        }
    }

    public void OpenScene()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
