using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;
using ButtonManager;
using DirectoryManager;
using ViewManager;

public class FileSelector : MonoBehaviour
{
    //Button prefab.
    public GameObject prefab;

    //General IO objects.
    public GameObject mainUI;
    public GameObject parent;
    public GameObject backButton;
    public GameObject searchField;
    public GameObject searchButton;
    public GridObjectCollection collection;

    //"Use selected file" screen objects.
    public GameObject noButton;
    public GameObject yesButton;
    public GameObject useFilePrompt;
    public GameObject useFileScreen;

    //Not DXF view objects.
    public GameObject continueButton;

    /*Instances of directoryManager, only keeps current and start alive due to 
    this projects application. If you want to keep multiple instances of currentDirectory
    alive for future use they would be added to a stack or similar.
    */
    directortManager startDirectory;
    directortManager currentDirectory;
    Stack pathHistory = new Stack();
    private string startPath = @"C:\Users\Axel\Desktop\DeromeTruss";

    UIButtons uiButtons = new UIButtons();
    string screen = "main";

    //Initializer, runs once.
    void Start()
    {
        uiButtons.add(backButton, "Back");
        uiButtons.add(noButton, "No");
        uiButtons.add(yesButton, "Yes");
        uiButtons.add(continueButton, "Contuine");

        //Sets the correct UI screen/view visible.
        useFileScreen.SetActive(false);
        mainUI.SetActive(true);

        /*Starts an instance of directoryManager for the start directory which is kept alive
        as startDirectory, although the instance which will be used in browsing is currentDirectory.
        */
        startDirectory = new directortManager(startPath, parent, prefab);
        startDirectory.updateCollection();
        currentDirectory = startDirectory;       
    }

    void updatePrompt(string text)
    {
        Transform child = useFilePrompt.transform.Find("IconAndText");
        Transform tmpObject = child.transform.Find("TextMeshPro");
        if (child == null || tmpObject == null)
            Debug.LogError("Child not found");
        else
        {
            TextMeshPro tmpComponent = tmpObject.GetComponent<TextMeshPro>();
            if (tmpComponent == null)
                Debug.LogError("No TextMeshPro component found");
            else
                tmpComponent.text = text;
        }
    }

    //Update runs once per frame.
    void Update()
    {
        switch(screen)
        {
            case "main":
                /*When going back in the directory tree, pathHistory.Count != 0 makes sure
            it cant go back past the start directroy. Sets the old instance of DirectoryManager
            to unactive before assigning currentDirectory a new instance of DirectoryManager.
            */
                if (uiButtons.update() == "Back")
                {
                    Debug.Log("Back!");
                    if (pathHistory.Count == 0)
                    {
                        currentDirectory = startDirectory;
                    }
                    else
                    {
                        currentDirectory.setActive(false);
                        currentDirectory = new directortManager(pathHistory.Pop().ToString(), parent, prefab);
                        currentDirectory.setActive(true);
                    }
                }

                /*Runs buttonStatus of DirectoryManager currently assigned to currentDirectory
                to check whether a button has been pressed, returns dict which is captured in dirctoryButton.
                Checks whether file or directory, 

                if file, make sure it's of typ .dxf and change view/screen to useFileScreen.

                if directory, set the setActive of the view of the current directory false, push the new
                directory path to stack, assign currentDirectory new instance of DirectoryManager with
                updated path. Set setActive true for the new view.
                */
                Dictionary<string, string> directoryButton = currentDirectory.buttonStatus();

                if (directoryButton != null)
                {
                    if (directoryButton["Type"] == "file")
                    {
                        mainUI.SetActive(false);
                        if (directoryButton["Name"].EndsWith(".dxf", true, null))
                        {
                            useFileScreen.SetActive(true);
                            continueButton.SetActive(true);
                            updatePrompt("Open file \"" + directoryButton["Name"] + "\"?");
                            screen = "useFile";
                        }
                        else
                        {
                            useFileScreen.SetActive(true);
                            continueButton.SetActive(true);
                            yesButton.SetActive(false);
                            noButton.SetActive(false);
                            updatePrompt("File is not .dxf!");
                            screen = "notDXF";
                        }
                    }

                    if (directoryButton["Type"] == "directory")
                    {
                        Debug.Log(directoryButton["Name"] + " : " + directoryButton["Type"]);
                        currentDirectory.setActive(false);
                        pathHistory.Push(currentDirectory.directoryPath);
                        currentDirectory = new directortManager(directoryButton["Path"], parent, prefab);
                        currentDirectory.setActive(true);
                    }
                }
                break;
            case "useFile":
                if (uiButtons.update() == "No")
                {
                    Debug.Log("no");
                    useFileScreen.SetActive(false);
                    mainUI.SetActive(true);
                }
                if (uiButtons.update() == "Yes")
                {
                    Debug.Log("yes");
                }
                break;
            case "notDXF":
                if (uiButtons.update() == "Continue")
                {
                    useFileScreen.SetActive(false);
                    mainUI.SetActive(true);
                }
                break;
        }
    }
}