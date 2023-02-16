using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;
using ButtonManager;

namespace DirectoryManager
{
    class directortManager : MonoBehaviour
    {
        private GameObject prefab;
        private GameObject parent;
        public string directoryPath { get; }
        private GridObjectCollection collection;

        //Directory for each button with its name as key
        private List<button> thisDirectory = new List<button>();

        //Loops the buttonManager instances for all the buttons and
        //checks whether any one of them have been pressed, will return
        //dict of name and type to caller
        public Dictionary<string, string> buttonStatus()
        {
            Dictionary<string, string> returnDict = new Dictionary<string, string>();
            foreach (button button in thisDirectory)
            {
                if (button.Button.pressed(button.PressState))
                {
                    Debug.Log("DM recorded TRUE!: " + button.Name);
                    returnDict.Add("Name", button.Name);
                    returnDict.Add("Type", button.Type);
                    returnDict.Add("Path", button.Path);
                    return (returnDict);
                }
            }
            return (null);
        }

        //Updates the grid collection, is called on visible changes
        public void updateCollection()
        {
            collection = parent.GetComponent<GridObjectCollection>();
            collection.UpdateCollection();
        }

        public void setActive(bool activity)
        {
            foreach (button button in thisDirectory)
                button.Button.button.SetActive(activity);

            updateCollection();
        }

        //Simple search function, called from generateEnviroment.cs, changes each gameObject button
        //state acoordingly to whether it conforms to searchTerm or not
        public void search(string searchTerm)
        {
            foreach (button button in thisDirectory)
            {
                if (!button.Name.Contains(searchTerm))
                    button.Button.button.SetActive(false);
                else
                    button.Button.button.SetActive(true);
            }
            updateCollection();
        }

        //Generates button class instances and adds them to a temporary list,
        //one instance per file and subdirectory in directory, adds relevant
        //informtion to said instances, type, name and path
        private List<button> getFilesAndDirectories(string directoryPath)
        {
            List<button> filesAndDirectories = new List<button>();

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
                tempButton.Name = Path.GetFileName(directory);
                filesAndDirectories.Add(tempButton);
            }

            foreach (string file in files)
            {
                button tempButton = new button();
                tempButton.Path = file;
                tempButton.Type = "file";
                tempButton.Name = Path.GetFileName(file);
                filesAndDirectories.Add(tempButton);
            }
            return filesAndDirectories;
        }

        //Constructor
        public directortManager(string Path, GameObject Parent, GameObject Prefab)
        {
            directoryPath = Path;
            parent = Parent;
            prefab = Prefab;
            Debug.Log("Setup");

            //Generates instances of local class button with relevant information
            thisDirectory = getFilesAndDirectories(directoryPath);
            //Loops thisDirectory list of button class instances and initiates instances of
            //buttonManager for Button property button

            foreach (button button in thisDirectory)
            {
                /*Issue with changing value of local variable from AddListener line thus the workaround
                *With using a fake constructor
                https://forum.unity.com/threads/onclick-addlistener-with-a-string-parameter.892210/*/
                button.Button = new buttonManager();
                button.PressState = button.Button.buttonManagerBuilder(prefab, parent, button.Name);
            }
            Debug.Log("DM constrctur done");
        }
    }
    class button
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
        public buttonManager Button { get; set; }
        public PressState PressState { get; set; }
    }
}
