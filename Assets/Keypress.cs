using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keypress : MonoBehaviour
{
    public GameObject upperplates;
    public GameObject lowerplates;
    public GameObject timber;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            upperplates.SetActive(true);
            timber.SetActive(true);
            lowerplates.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            lowerplates.SetActive(true);
            upperplates.SetActive(false);
            timber.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            timber.SetActive(true);
            lowerplates.SetActive(false);
            upperplates.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            upperplates.SetActive(true);
            lowerplates.SetActive(false);
            timber.SetActive(false);

        }
        else if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            upperplates.SetActive(true);
            timber.SetActive(true);
            lowerplates.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            timber.SetActive(true);
            upperplates.SetActive(true);
            lowerplates.SetActive(true);
        }
    }
}
