using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;

public class generateEnviroment : MonoBehaviour
{
    public GameObject prefab;
    public GameObject parent;
    public GridObjectCollection collection;

    public Dictionary<string, GameObject> buttons = new Dictionary<string, GameObject>();


    //Path to read from, will changed to be read from a config file
    public string directoryPath = @"C:\Users\Axel\Desktop\DeromeTruss";

    //Returns a string list of every folder and file in the selected directroy
    public List<string> GetFilesAndDirectories(string directoryPath)
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

    void Search(string searchTerm)
    {
        //Iterates over the keys and checks whether each key contains
        //the search term, if it does not contain the gameobject behind
        //said key is deactivated, else its activated
        foreach (string name in buttons.Keys)
        {
            if (!name.Contains(searchTerm))
            {
                buttons[name].SetActive(false);
            }
            else
            {
                buttons[name].SetActive(true);
            }
        }
        collection = parent.GetComponent<GridObjectCollection>();
        collection.UpdateCollection();
    }

    void Start()
    {
        //Runs the files and directories function and saves the return in a string list
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


        //Updates the grid collection
        collection = parent.GetComponent<GridObjectCollection>();
        collection.UpdateCollection();

    }

    // Update is called once per frame
    void Update()
    {
        Search("Microsoft");
    }
}
