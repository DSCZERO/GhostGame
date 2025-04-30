using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    public Animator targetAnimator;
    public string animatorBoolName = "open";

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.name);

        if (other.CompareTag("Telekinesis"))
        {
            Debug.Log("Telekinesis object detected.");
            if (targetAnimator != null)
                targetAnimator.SetBool(animatorBoolName, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Telekinesis"))
        {
            Debug.Log("Telekinesis object left.");
            if (targetAnimator != null)
                targetAnimator.SetBool(animatorBoolName, false);
        }
    }
}
