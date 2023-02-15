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
        GameObject prefab;
        GameObject parent;
        string directoryPath;

        private GridObjectCollection collection;

        private Dictionary<string, button> buttons = new Dictionary<string, button>();

        private Dictionary<string, button> GetFilesAndDirectories(string directoryPath)
        {
            Dictionary<string, button> filesAndDirectories = new Dictionary<string, button>();

            //If the path happens to be invalid
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogError("Invalid path");
                return null;
            }

            //One string array for directories and one for files
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

        public void updateCollection()
        {
            //Updates the grid collection
            collection = parent.GetComponent<GridObjectCollection>();
            collection.UpdateCollection();
        }
        public void setActive(bool activity)
        {
            foreach (button thisButton in buttons.Values)
                thisButton.Button.SetActive(activity);
        }
        public string buttonStatus()
        {
            foreach (KeyValuePair<string, button> buttonPair in buttons)
            {
                //Gets the Interactable component for the current string, GameObject pair
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
                lastReturn = lastClickedButtonName;
                return lastClickedButtonName;
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
                //Debug due to no implemented functionality
                Debug.Log(buttonName + " button has been pressed.");

                // Set the last clicked button name
                lastClickedButtonName = buttonName;
            }
        }


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