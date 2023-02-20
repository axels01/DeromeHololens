/*
 * 2023 Axel Östergren
 * axel.ostergren@student.hv.se
 * 
 * 
 * This script is deprecated, see ButtonManager.cs and DirectoryManager.cs.
 */


using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;

namespace Buttons   
{
    class ButtonHandler : MonoBehaviour
    {
        private GameObject prefab;
        private GameObject parent;
        public string directoryPath { get; }
        private GridObjectCollection collection;

        //Directory for each button with its name as key
        private Dictionary<string, button> buttons = new Dictionary<string, button>();
        
        /*Generates dictionary of button class objects, one for each file or folder in directory
         * First check whether the given directroy is valid, else returns error and null
         * Generates string arrays, one containing files and one for directories
         * 
         * Loops through directories and files arrays, respectively, adds definitions in
         * the directory for each string in the array and creates a button object where
         * path and type is set acoordingily
         */
        private Dictionary<string, button> GetFilesAndDirectories(string directoryPath)
        {
            Dictionary<string, button> filesAndDirectories = new Dictionary<string, button>();

            if (!Directory.Exists(directoryPath))
            {
                Debug.LogError("Invalid path");
                return null;
            }

            string[] directories = Directory.GetDirectories(directoryPath);
            string[] files = Directory.GetFiles(directoryPath);

            foreach (string directory in directories)
            {
                button tempButton = new button();
                tempButton.Path = directory;
                tempButton.Type = "directory";
                filesAndDirectories.Add(Path.GetFileName(directory), tempButton);
            }

            foreach (string file in files)
            {
                button tempButton = new button();
                tempButton.Path = file;
                tempButton.Type = "file";
                filesAndDirectories.Add(Path.GetFileName(file), tempButton);
            }

            return filesAndDirectories;
        }

        //Updates the grid collection, is called on visible changes
        public void updateCollection()
        {
            collection = parent.GetComponent<GridObjectCollection>();
            collection.UpdateCollection();
        }

        //Sets the active state of all gameObjects the current object of this class governs over
        //used when going between dictionaries, called from generateEnviroment.cs
        public void setActive(bool activity)
        {
            foreach (button thisButton in buttons.Values)
                thisButton.Button.SetActive(activity);

            updateCollection();
        }

        //Simple search function, called from generateEnviroment.cs, changes each gameObject button
        //state acoordingly to whether it conforms to searchTerm or not
        public void search(string searchTerm)
        {
            foreach (KeyValuePair<string, button> buttonPair in buttons)
            {
                if (!buttonPair.Key.Contains(searchTerm))
                    buttonPair.Value.Button.SetActive(false);

                else
                    buttonPair.Value.Button.SetActive(true);
            }
            updateCollection();
        }

        /*Called to from generateEnviroment.cs checking whether a button has been pressed
        *returns a dictionary of values concering the pressed button, else null
        *
        *Loops each keyValuePair and gets the gameObject component containing the button
        *if the component is not null the "Pressed" value of the button is set false and a
        *new event listener is started for the button, instance of ButtonClicked.
        *
        *If there is a difference in the "latesteClickedButtonName" and "lastReturn" strings, do 
        *a new return of dict containing name, type and path.
        */
        public Dictionary<string, string> buttonStatus()
        {
            foreach (KeyValuePair<string, button> buttonPair in buttons)
            {
                Interactable buttonComponent = buttonPair.Value.Button.GetComponent<Interactable>();

                if (buttonComponent != null)
                {
                    //Boolean dictionary updated to false to remove the possibility for repeat clicks during one click event
                    buttonPair.Value.Pressed = false;
                    //Starts a new event listener for the button
                    buttonComponent.OnClick.AddListener(() => ButtonClicked(buttonPair.Key));
                }
            }

            // Return the button name of the last clicked button
            if (lastClickedButtonName != lastReturn)
            {
                Dictionary<string, string> returnDict = new Dictionary<string, string>();
                returnDict.Add("Name", lastClickedButtonName);
                returnDict.Add("Type", buttons[lastClickedButtonName].Type);
                returnDict.Add("Path", buttons[lastClickedButtonName].Path);

                lastReturn = lastClickedButtonName;
                return returnDict;
            }
            else
                return null;
        }

        private string lastClickedButtonName = null;
        private string lastReturn = null;

        //Event listener
        private void ButtonClicked(string buttonName)
        {
            //Negates multiple recorded clicks from one click event
            if (!buttons[buttonName].Pressed)
            {
                buttons[buttonName].Pressed = true;

                // Set the last clicked button name
                lastClickedButtonName = buttonName;
            }
        }

        /* Calls GetFilesAndDirectories tot generate intial dict of "button" class objects
         * 
         * Loops the keys and creates a gameObject of a prefab where its "TextMeshPro" component
         * text variable is set to the buttons name, the gameObject property of the button class instance
         * is then set to new gameObject 
         */
        private void setup()
        {
            buttons = GetFilesAndDirectories(directoryPath);

            foreach (string name in buttons.Keys)
            {
                //Creates one new object of a prefab per item in the string list above
                GameObject newItem = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, parent.transform);

                //Gets the child containing the TextMeshPro component
                Transform child = newItem.transform.Find("IconAndText");
                Transform tmpObject = child.transform.Find("TextMeshPro");
                if (child == null || tmpObject == null)
                {
                    Debug.LogError("Child not found");
                }
                else
                {
                    TextMeshPro tmpComponent = tmpObject.GetComponent<TextMeshPro>();
                    if (tmpComponent == null)
                    {
                        Debug.LogError("No TextMeshPro component found");
                    }
                    else
                    {
                        tmpComponent.text = name;
                    }
                }
                buttons[name].Button = newItem;
            }
        }

        //Constructor
        public ButtonHandler(string Path, GameObject Parent, GameObject Prefab)
        {
            directoryPath = Path;
            parent = Parent;
            prefab = Prefab;
            Debug.Log("Setup");
            setup();
        }
    }

    class button
        {
        public string Type { get; set; }
        public string Path { get; set; }
        public GameObject Button { get; set; }
        public bool Pressed { get; set; }
    }
}