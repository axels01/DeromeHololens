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
        private Dictionary<string, GameObject> buttons = new Dictionary<string, GameObject>();
        private Dictionary<string, bool> buttonPressedDictionary = new Dictionary<string, bool>();

        private List<string> GetFilesAndDirectories(string directoryPath)
        {
            List<string> filesAndDirectories = new List<string>();

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
                //Makes sure the initial path is not recorded in the string list 
                string directoryName = Path.GetDirectoryName(directory);
                filesAndDirectories.Add(Path.GetFileName(directory));
            }

            foreach (string file in files)
            {
                filesAndDirectories.Add(Path.GetFileName(file));
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
            foreach(GameObject button in buttons.Values)
                button.SetActive(activity);
        }
        public string buttonStatus()
        {
            foreach (KeyValuePair<string, GameObject> buttonPair in buttons)
            {
                //Gets the Interactable component for the current string, GameObject pair
                Interactable buttonComponent = buttonPair.Value.GetComponent<Interactable>();

                if (buttonComponent != null)
                {
                    //Boolean dictionary updated to false to remove the possibility for repeat clicks during one click event
                    buttonPressedDictionary[buttonPair.Key] = false;
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
            if (!buttonPressedDictionary[buttonName])
            {
                buttonPressedDictionary[buttonName] = true;
                //Debug due to no implemented functionality
                Debug.Log(buttonName + " button has been pressed.");

                // Set the last clicked button name
                lastClickedButtonName = buttonName;
            }
        }


        private void setup()
        {
            List<string> filesAndDirectories = GetFilesAndDirectories(directoryPath);

            foreach (string name in filesAndDirectories)
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
                buttons.Add(name, newItem);
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
}