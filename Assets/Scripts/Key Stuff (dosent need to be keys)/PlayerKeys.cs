using System.Collections.Generic;
using UnityEngine;

public class PlayerKeys : MonoBehaviour
{
    public static PlayerKeys Instance;

    private HashSet<string> collectedKeys = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddKey(string keyID)
    {
        collectedKeys.Add(keyID);
        Debug.Log($"Collected key: {keyID}");
    }

    public bool HasKey(string keyID)
    {
        return collectedKeys.Contains(keyID);
    }
}
