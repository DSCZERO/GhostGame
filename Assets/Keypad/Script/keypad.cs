using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class keypad : MonoBehaviour
{
    public GameObject objectToEnable;
    public GameObject objectToDestroy;

    [Header("Keypad Settings")]
    public string curPassword = "123";
    public string input;
    public Text displayText;
    public AudioSource audioData;

    // Reference to the GhostMode script
    public GhostMode ghostMode;

    public MonoBehaviour cameraMovement;

    //Local private variables
    private bool keypadScreen;
    private float btnClicked = 0;
    private float numOfGuesses;

    // Start is called before the first frame update
    void Start()
    {
        btnClicked = 0; // No of times the button was clicked
        numOfGuesses = curPassword.Length; // Set the password length.
    }

    // Update is called once per frame
    void Update()
    {
        if (btnClicked == numOfGuesses)
        {
            if (input == curPassword)
            {
                Debug.Log("Correct Password!");

                // If an object is set to be destroyed, disable it
                if (objectToDestroy != null)
                {
                    objectToDestroy.SetActive(false);
                }

                ExitKeypad();
                
                // Simulate pressing Q to quit the keypad
                objectToEnable.SetActive(false);
                keypadScreen = false;
                Cursor.visible = false;
                input = "";
                btnClicked = 0;
                displayText.text = input.ToString();
            }
            else
            {
                // Reset input if the password is incorrect
                input = "";
                displayText.text = input.ToString();
                audioData.Play();
                btnClicked = 0;
            }
        }
    }

    void OnGUI()
    {
        // If the player is in ghost mode, skip keypad interaction
        if (ghostMode != null && ghostMode.IsInGhostMode)
        {
            return;
        }

        // Action for clicking keypad on screen
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                var selection = hit.transform;

                if (selection.CompareTag("keypad"))
                {
                    keypadScreen = true;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    objectToEnable.SetActive(true);

                    if (cameraMovement != null)
                    {
                        cameraMovement.enabled = false;
                    }
                }
            }
        }
    }

    public void ValueEntered(string valueEntered)
    {
        switch (valueEntered)
        {
            case "Q": // QUIT
                ExitKeypad();
                break;

            case "C": // CLEAR
                input = "";
                btnClicked = 0;
                displayText.text = input.ToString();
                break;

            default: // Append value to input when a button is clicked
                btnClicked++;
                input += valueEntered;
                displayText.text = input.ToString();
                break;
        }
    }

    void ExitKeypad()
    {
        objectToEnable.SetActive(false);
        btnClicked = 0;
        keypadScreen = false;
        input = "";
        displayText.text = input.ToString();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (cameraMovement != null)
        {
            cameraMovement.enabled = true;
        }
    }
}
