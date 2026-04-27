using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMusic : MonoBehaviour
{
    void Start()
    {
        AudioManager.instance.Play("Test");
    }
}
