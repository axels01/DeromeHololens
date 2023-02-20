/*
 * 2023 Axel Östergren
 * axel.ostergren@student.hv.se
 * 
 * 
 * This script is deprecated, see FileSelector.cs and ViewManager.cs.
 */


using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;
using Buttons;

/*Todo
 * Add logic if the button pressed is file
 * Add logic for asking whether or not to open selected file
 * 
 * Add "back" button
 * Add logic for a "back" button
 * 
 * Add search input and use ButtonHandler.search(string);
 * 
 * 
 */


public class generateEnviroment : MonoBehaviour
{
    public GameObject prefab;
    public GameObject parent;
    public GameObject backButton;
    public GameObject selectButton;
    public GameObject searchField;
    public GameObject searchButton;
    public GridObjectCollection collection;

    ButtonHandler startDirectory;
    ButtonHandler currentDirectory;
    //Path to read from, will changed to be read from a config file
    private string startPath = @"C:\Users\Axel\Desktop\DeromeTruss";
    //Stores the history of paths, push when entering a new path, pop when going back
    Stack pathHistory = new Stack();

    private Dictionary<GameObject, bool> buttonsPressedDict = new Dictionary<GameObject, bool>();
   
    void Start()
    {
        //Starts a new ButtonHandler for the start directroy
        startDirectory = new ButtonHandler(startPath, parent, prefab);
        startDirectory.updateCollection();
        currentDirectory = startDirectory;
    }


    bool pressed(GameObject buttonToCheck)
    {
        Interactable buttonComponent = buttonToCheck.GetComponent<Interactable>();

        if (buttonComponent != null)
        {
            buttonsPressedDict[buttonToCheck] = false;
            buttonComponent.OnClick.AddListener(() => ButtonClicked(buttonToCheck));
        }
        if (lastClickedButtonName != null)
        {
            lastClickedButtonName = null;
            return true;
        }
        else
            return false;
    }

    private GameObject lastClickedButtonName = null;

    private void ButtonClicked(GameObject buttonToCheck)
    {
        //Negates multiple recorded clicks from one click event
        if (!buttonsPressedDict[buttonToCheck])
        {
            buttonsPressedDict[buttonToCheck]= true;
            // Set the last clicked button name
            lastClickedButtonName = buttonToCheck;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (pressed(backButton))
        {
            Debug.Log("Back!");
            if (pathHistory.Count != 0)
            {
                currentDirectory.setActive(false);
                currentDirectory = new ButtonHandler(pathHistory.Pop().ToString(), parent, prefab);
                currentDirectory.setActive(true);
            }
        }

        Dictionary<string, string> directoryButtons = currentDirectory.buttonStatus();
        if (directoryButtons != null)
        {
            if (directoryButtons["Type"] == "file")
            {
                Debug.Log(directoryButtons["Name"] + " : " + directoryButtons["Type"]);
                /*Add logic 
                 * Check if the file is .dxf
                 * if it is, pass to ParseDXF and set this script inactive
                 */
            }

            if (directoryButtons["Type"] == "directory")
            {
                /*if the button pressed is a directory, set the currentDirectory
                *to false, this only makes a difference if the user is currently standing in
                *startDirectory as the buttons from said directory will be shown alongside the
                *ones from the new directory
                */
                Debug.Log(directoryButtons["Name"] + " : " + directoryButtons["Type"]);
                currentDirectory.setActive(false);

                /*starts a new instance of ButtonHandler under currentDirectory variable with
                *the new path, then updates then updateCollection and setActive for the buttons
                *from the new directory
                */
                pathHistory.Push(currentDirectory.directoryPath);
                currentDirectory = new ButtonHandler(directoryButtons["Path"], parent, prefab);
                currentDirectory.setActive(true);
            }
        }
    }
}
