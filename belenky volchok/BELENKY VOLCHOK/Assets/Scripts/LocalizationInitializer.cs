using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using System.Collections;

public class LocalizationInitializer : MonoBehaviour
{
    IEnumerator Start()
    {
        // Wait for localization to fully initialize
        yield return LocalizationSettings.InitializationOperation;
        
        // Now load your actual first scene
        SceneManager.LoadScene("Scene1"); // or whatever your scene is called
    }
}
