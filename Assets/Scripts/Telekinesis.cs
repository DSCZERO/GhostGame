using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Telekenisis : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.T)) {
            
        }
    }

    void OnTriggerEnter (Collider other)
    {
        Debug.Log("You've entered telekenis range");


    }
}
