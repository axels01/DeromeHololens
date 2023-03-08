/*
 * 2023 Axel Östergren
 * axel.ostergren@student.hv.se
 *
 *
 * 2023 Arvid Albinsson
 * arvid.albinsson@student.hv.se
 * Added the functionality for the searchfunction.
 */



using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
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
    public GameObject keyboard;
    public MixedRealityKeyboard keyboardComponent;
    public GameObject toggleKeyboard;
    public GameObject searchField;
    public TMP_InputField searchfieldcomponent;
    public GameObject searchButton;
    public GridObjectCollection collection;

    //"Use selected file" screen objects.
    public GameObject noButton;
    public GameObject yesButton;
    public GameObject useFilePrompt;
    public GameObject useFileScreen;

    //Not DXF view objects.
    public GameObject notDXFPrompt;
    public GameObject continueButton;
    public GameObject notDXFScreen;

    /*Instances of directoryManager, only keeps current and start alive due to 
    this projects application. If you want to keep multiple instances of currentDirectory
    alive for future use they would be added to a stack or similar.
    */
    directortManager startDirectory;
    directortManager currentDirectory;
    Stack pathHistory = new Stack();
    
    public string selectedFile = null;
    public bool done = false;
    private string startPath = @"E:\";
    //private string startPath = @"C:\Users\Axel\Desktop\DeromeTruss";
    //private string startPath = @"C:\Users\Arvid\OneDrive\Skrivbord\DeromeTruss";
    UIButtons uiButtons = new UIButtons();
    string screen = "main";
    public bool keyboardCommit = false;

    //Initializer, runs once.
    void Start()
    {
        keyboardComponent = keyboard.GetComponent<MixedRealityKeyboard>();
        searchfieldcomponent = searchField.GetComponent<TMP_InputField>();

        uiButtons.add(backButton, "Back");
        uiButtons.add(noButton, "No");
        uiButtons.add(yesButton, "Yes");
        uiButtons.add(continueButton, "Continue");
        uiButtons.add(toggleKeyboard, "ToggleKeyboard");

        //Sets the correct UI screen/view visible.
        useFileScreen.SetActive(false);
        notDXFScreen.SetActive(false);
        notDXFPrompt.SetActive(false);
        mainUI.SetActive(true);

        /*Starts an instance of directoryManager for the start directory which is kept alive
        as startDirectory, although the instance which will be used in browsing is currentDirectory.
        */
        startDirectory = new directortManager(startPath, parent, prefab);
        startDirectory.updateCollection();
        currentDirectory = startDirectory;
        keyboardComponent.OnCommitText.AddListener(() => keyboardCommit = true);
        searchfieldcomponent.onValueChanged.AddListener((string data) => keyboardCommit = true);
    }

    void updatePrompt(GameObject prompt ,string text)
    {
        Transform child = prompt.transform.Find("IconAndText");
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
                string btn = uiButtons.update();
                if (btn == "ToggleKeyboard")
                {
                    Debug.Log("Keyboard :)");
                    if (!keyboardComponent.Visible)
                        keyboardComponent.ShowKeyboard("", false);
                    else
                        keyboardComponent.HideKeyboard();
                }

                if (keyboardCommit == true)
                {
                    string temp = searchfieldcomponent.text;
                    Debug.Log("Keyboard: " + temp);
                    currentDirectory.search(temp);
                    keyboardCommit = false;
                }
                else if (btn == "Back")
                {
                    searchfieldcomponent.text = "";
                    Debug.Log("Back!");
                    if (pathHistory.Count > 0)
                    {
                        currentDirectory.destroy();
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
                    Debug.Log(directoryButton["Name"]);
                    if (directoryButton["Type"] == "file")
                    {
                        mainUI.SetActive(false);
                        if (directoryButton["Name"].EndsWith(".dxf", true, null))
                        {
                            useFileScreen.SetActive(true);
                            updatePrompt(useFilePrompt, "Öppna \"" + directoryButton["Name"] + "\"?");
                            selectedFile = directoryButton["Path"];
                            screen = "useFile";
                        }
                        else
                        {
                            notDXFScreen.SetActive(true);
                            notDXFPrompt.SetActive(true);
                            updatePrompt(notDXFPrompt, "Vald fil är inte en .dxf!");
                            screen = "notDXF";
                        }
                    }

                    if (directoryButton["Type"] == "directory")
                    {
                        currentDirectory.destroy();
                        searchfieldcomponent.text = "";
                        Debug.Log(directoryButton["Name"] + " : " + directoryButton["Type"]);
                        currentDirectory.setActive(false);
                        pathHistory.Push(currentDirectory.directoryPath);
                        currentDirectory = new directortManager(directoryButton["Path"], parent, prefab);
                        currentDirectory.setActive(true);
                    }
                }
                break;
            case "useFile":
                string button = uiButtons.update();
                if (button == "No")
                {
                    Debug.Log("no");
                    useFileScreen.SetActive(false);
                    mainUI.SetActive(true);
                    screen = "main";
                }
                if (button  == "Yes")
                {
                    done = true;
                    Debug.Log("yes");
                }
                break;
            case "notDXF":
                if (uiButtons.update() == "Continue")
                {
                    notDXFScreen.SetActive(false);
                    mainUI.SetActive(true);
                    screen = "main";
                }
                break;
        }
    }
}
