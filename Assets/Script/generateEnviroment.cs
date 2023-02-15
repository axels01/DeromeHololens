using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;
using Buttons;

public class generateEnviroment : MonoBehaviour
{
    public GameObject prefab;
    public GameObject parent;
    public GridObjectCollection collection;


    ButtonHandler startDirectory;
    //Path to read from, will changed to be read from a config file
    public string directoryPath = @"C:\Users\Axel\Desktop\DeromeTruss";
   
    void Start()
    {
        //Starts a new ButtonHandler for the start directroy
        startDirectory = new ButtonHandler(directoryPath, parent, prefab);
        startDirectory.updateCollection();        
    }

    // Update is called once per frame
    void Update()
    {
        string button = startDirectory.buttonStatus();
        if (button != null)
            Debug.Log("Main: " + button);

        if (button == ".vs")
        {
            startDirectory.setActive(false);
        }

    }
}
