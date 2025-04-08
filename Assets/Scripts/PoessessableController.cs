using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossessableController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotationSpeed = 200f;

    private bool isPossessed = false;

    public void SetPossessed(bool value)
    {
        isPossessed = value;
    }

    void Update()
    {
        if (!isPossessed) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v);
        transform.Translate(move * moveSpeed * Time.deltaTime);

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(0, mouseX * rotationSpeed * Time.deltaTime, 0);

        Camera.main.transform.Rotate(-mouseY * rotationSpeed * Time.deltaTime, 0, 0);
    }
}