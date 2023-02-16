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

    /*Instances of directoryManager, only keeps current and start alive due to 
    this projects application. If you want to keep multiple instances of currentDirectory
    alive for future use they would be added to a stack or similar.
    */
    directortManager startDirectory;
    directortManager currentDirectory;
    Stack pathHistory = new Stack();
    
    private string startPath = @"C:\Users\Axel\Desktop\DeromeTruss";
    private Dictionary<GameObject, UIButton> buttons = new Dictionary<GameObject, UIButton>();
    
    
    //Initializer, runs once.
    void Start()
    {
        //Sets the correct UI screen/view visible.
        useFileScreen.SetActive(false);
        mainUI.SetActive(true);


        /*Starts an instance of directoryManager for the start directory which is kept alive
        as startDirectory, although the instance which will be used in browsing is currentDirectory.
        */
        startDirectory = new directortManager(startPath, parent, prefab);
        startDirectory.updateCollection();
        currentDirectory = startDirectory;



        /*Creates instances of UIButton datatype for each button and adds to buttons dictionary.
        Loops through the dictionary and assigns a instance of buttonManager and pressState to 
        each button. 
        
        Uses a fake constructor for buttonManager, which is further explained in ButtonManager.cs. 
        */
        buttons.Add(noButton, new UIButton());
        buttons.Add(yesButton, new UIButton());
        buttons.Add(backButton, new UIButton());

        foreach (KeyValuePair<GameObject, UIButton> button in buttons)
        {
            button.Value.buttonManager = new buttonManager();
            button.Value.pressState = button.Value.buttonManager.buttonManagerBuilder(button.Key);
        }
    }

    //Update runs once per frame.
    void Update()
    {
        /*If statement regarding which screen/view should be show, should probably be replacd with switch case.
        mainUI represents the view/screen showing on start.
        */
        if (mainUI.activeSelf)
        {
            /*When going back in the directory tree, pathHistory.Count != 0 makes sure
            it cant go back past the start directroy. Sets the old instance of DirectoryManager
            to unactive before assigning currentDirectory a new instance of DirectoryManager.
            */
            if (buttons[backButton].buttonManager.pressed(buttons[backButton].pressState))
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
                    Debug.Log(directoryButton["Name"] + " : " + directoryButton["Type"]);
                    if (directoryButton["Name"].EndsWith(".dxf", true, null))
                    {
                        mainUI.SetActive(false);
                        useFileScreen.SetActive(true);
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
                                tmpComponent.text = "Use file: " + directoryButton["Name"] + "?";
                        }
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
        }
        
        /*View/screen when user has clicked on a file.
        Allows user to cancel or contine via yes/no buttons.
        */
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

//UIbutton datatype.
class UIButton
{
    public buttonManager buttonManager { get; set; }
    public PressState pressState { get; set; }
}
