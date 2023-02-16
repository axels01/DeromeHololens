using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;
using ButtonManager;
using DirectoryManager;
using UIManager;

public class FileSelector : MonoBehaviour
{
    //Button prefab
    public GameObject prefab;

    //General IO objects
    public GameObject mainUI;
    public GameObject parent;
    public GameObject backButton;
    public GameObject searchField;
    public GameObject searchButton;
    public GridObjectCollection collection;

    //Use selected file
    public GameObject noButton;
    public GameObject yesButton;
    public GameObject useFilePrompt;
    public GameObject useFileScreen;

    //Instances of directoryManager
    directortManager startDirectory;
    directortManager currentDirectory;
    //Path to read from, will changed to be read from a config file
    private string startPath = @"C:\Users\Axel\Desktop\DeromeTruss";
    //Stores the history of paths, push when entering a new path, pop when going back
    Stack pathHistory = new Stack();
    private Dictionary<GameObject, UIButton> buttons = new Dictionary<GameObject, UIButton>();

    void Start()
    {
        //Sets the correct UI visible
        useFileScreen.SetActive(false);
        mainUI.SetActive(true);


        //Starts a new ButtonHandler for the start directroy
        startDirectory = new directortManager(startPath, parent, prefab);
        startDirectory.updateCollection();
        currentDirectory = startDirectory;



        //Populates buttons with instances of buttonManager for the buttons.
        buttons.Add(noButton, new UIButton());
        buttons.Add(yesButton, new UIButton());
        buttons.Add(backButton, new UIButton());

        foreach (KeyValuePair<GameObject, UIButton> button in buttons)
        {
            button.Value.buttonManager = new buttonManager();
            button.Value.pressState = button.Value.buttonManager.buttonManagerBuilder(button.Key);
        }

        buttons[noButton].pressState = buttons[noButton].buttonManager.buttonManagerBuilder(noButton);
    }

    void Update()
    {
        if (mainUI.activeSelf)
        {
            if (buttons[backButton].buttonManager.pressed(buttons[backButton].pressState))
            {
                Debug.Log("Back!");
                if (pathHistory.Count != 0)
                {
                    currentDirectory.setActive(false);
                    currentDirectory = new directortManager(pathHistory.Pop().ToString(), parent, prefab);
                    currentDirectory.setActive(true);
                }
            }

            Dictionary<string, string> directoryButton = currentDirectory.buttonStatus();
            if (directoryButton != null)
            {
                if (directoryButton["Type"] == "file")
                {
                    Debug.Log(directoryButton["Name"] + " : " + directoryButton["Type"]);
                    /*Add logic 
                     * Check if the file is .dxf
                     * if it is, pass to ParseDXF and set this script inactive
                     */

                    //if file ends with .dxf, ignoreCase = true, culture = null
                    if (directoryButton["Name"].EndsWith(".dxf", true, null))
                    {
                        mainUI.SetActive(false);
                        useFileScreen.SetActive(true);
                    }
                }

                if (directoryButton["Type"] == "directory")
                {
                    /*if the button pressed is a directory, set the currentDirectory
                    *to false, this only makes a difference if the user is currently standing in
                    *startDirectory as the buttons from said directory will be shown alongside the
                    *ones from the new directory
                    */
                    Debug.Log(directoryButton["Name"] + " : " + directoryButton["Type"]);
                    currentDirectory.setActive(false);

                    /*starts a new instance of ButtonHandler under currentDirectory variable with
                    *the new path, then updates then updateCollection and setActive for the buttons
                    *from the new directory
                    */
                    pathHistory.Push(currentDirectory.directoryPath);
                    currentDirectory = new directortManager(directoryButton["Path"], parent, prefab);
                    currentDirectory.setActive(true);
                }
            }
        }

        if (useFileScreen.activeSelf)
        {
            if (buttons[noButton].buttonManager.pressed(buttons[noButton].pressState))
            {
                Debug.Log("no");
                useFileScreen.SetActive(false);
                mainUI.SetActive(true);
            }

            if (buttons[yesButton].buttonManager.pressed(buttons[yesButton].pressState))
            {
                Debug.Log("yes");
            }
        }
    }
}

class UIButton
{
    public buttonManager buttonManager { get; set; }
    public PressState pressState { get; set; }
}
