/*
 * 2023 Axel Östergren
 * axel.ostergren@student.hv.se
 */


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
        private GameObject scroll;
        private GridObjectCollection collection;

        
        public string directoryPath { get; }
        private List<button> thisDirectory = new List<button>();

        /*Loops the buttonManager instances for all the buttons and
        checks whether any one of them have been pressed, will return
        dict of name, type and path to caller, else returns null.
        */
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
            collection.UpdateCollection();
        }
            
        //Changes setActive for all button GameObjects
        public void setActive(bool activity)
        {
            foreach (button button in thisDirectory)
                button.Button.button.SetActive(activity);

            updateCollection();
        }

        /*Simple search function, called from Fileselector.cs, changes each gameObject button
        setActive acoordingly to whether it conforms to searchTerm or not.
        *
        * 2023 Arvid Albinsson
        * arvid.albinsson@student.hv.se
        * Got the input inputTMP to function with the script so the searchfunctionality is dynamic and it refreshes when you enter a folder.
        */
        public void search(string searchTerm)
        {
            Debug.Log("Searchterm *" + searchTerm +"*");
            if (searchTerm == "" || searchTerm == null)
            { 
                foreach (button button in thisDirectory)
                {
                    button.Button.button.SetActive(false);
                }
                updateCollection();
                foreach (button button in thisDirectory)
                {
                    button.Button.button.SetActive(true);
                }
            }
            else
            {
                foreach (button button in thisDirectory)
                {
                    if (!button.Name.ToUpper().StartsWith(searchTerm.ToUpper()))
                        button.Button.button.SetActive(false);
                    else
                        button.Button.button.SetActive(true);
                }
            }
            updateCollection();
        }

        //Called to by FileSelector.cs, loops all button class instances and runs their
        //respective local destroy functions to remove unused gameObjects.
        public void destroy()
        {
            foreach (button button in thisDirectory)
                button.Button.destroy();
            Destroy(scroll);
        }

        /*Generates button class instances and adds them to a temporary list,
        one instance per file and subdirectory in directory, adds relevant
        informtion to instances, type, name and path.
        */
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

        /*Constructor
        Calls getFielsAndDirectories with relevant path, return is stored in
        thisDirectory, list of instances of button class. Loops all buttons in the
        generated list, gives each a new buttonManager instance, also assigns 
        button.PressState with new intsance of PressState, this is done via a fake
        constructor buttonManagerBuilder inside of instance of buttonManager, this 
        due to issues with regards to accessing local variables from Addlistner event.
        This is a workaround and should be updated to make the constructor a real one.
        
        Information regarind the issue:
        https://forum.unity.com/threads/onclick-addlistener-with-a-string-parameter.892210/
        */
        public directortManager(string Path, GameObject Prefab, GameObject Scroll)
        {
            scroll = Scroll;
            directoryPath = Path;
            parent = Scroll.transform.GetChild(1).GetChild(0).gameObject;
            prefab = Prefab;
            collection = parent.GetComponent<GridObjectCollection>();
            Debug.Log("Setup");
            
            thisDirectory = getFilesAndDirectories(directoryPath);

            foreach (button button in thisDirectory)
            {                
                button.Button = new buttonManager();
                button.PressState = button.Button.buttonManagerBuilder(prefab, parent, button.Name, button.Type);
            }
            updateCollection();
            Debug.Log("DM constrctur done");
        }
    }
    
    //button datatype, used locally.
    class button
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
        public buttonManager Button { get; set; }
        public PressState PressState { get; set; }
    }
}
