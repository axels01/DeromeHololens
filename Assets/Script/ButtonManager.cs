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

namespace ButtonManager
{
    /*Class PressState, to make sure multiple clicks aren't recorded
    from one click event. Workaround due to issue explained above
    buttonManagerBuildr function in buttonManager class bellow.
    */
    public class PressState
    {
        public bool isPressed = false;
        public void reset()
        {
            isPressed = false;
        }
           
        public void SetPressed()
        {
            isPressed = true;
        }
    }
    
    public class buttonManager : MonoBehaviour
    {
        public GameObject button { get; set; }
        public bool isPressed = false;

        //Function called from instance intiator to check if button has been pressed, returns bool.
        public bool pressed(PressState pressState)
        {
            bool result = pressState.isPressed;
            if (result)
                Debug.Log("BM is: " + result);
            pressState.reset();
            return result;
        }

        /*Attaches a listener to the Interactable of the gameObject, runs function in 
        pressState class on press event due to issue explained bellow. 
        */
        private void attachListener(PressState pressState)
        {
            Interactable interactable = button.GetComponent<Interactable>();
            interactable.OnClick.AddListener(() => Debug.Log("Interactable clicked"));
            if (interactable != null)
            {
                interactable.OnClick.AddListener(() => pressState.SetPressed());
            }
        }
        
        /*Fake constructor for instance of buttonManager class. Can be used to create a new 
        button from prefab and provided information or to only assign a listener to an already
        existing button.
        
        The constructor is fake due to issues with regards to accessing local variables from 
        Addlistner event. This is a workaround and should be updated to make the constructor a real one.
        
        Information regarind the issue:
        https://forum.unity.com/threads/onclick-addlistener-with-a-string-parameter.892210/
        
        First if-statement only attaches listener, for already created button.
        Second if-statement creates a new button GameObject from prefab and a name under a parent gameObject.
        Includes some error catching aswell.
        */
        public PressState buttonManagerBuilder(GameObject gameObject, GameObject parent = null, string name = null)
        {
            PressState pressState = new PressState();
            if (parent == null && name == null)
            {
                button = gameObject;
                attachListener(pressState);
            }
            else if (parent != null && name != null)
            {
                // Runs if the first argument is a prefab, i.e if it's called from DirectoryManager.cs
                button = Instantiate(gameObject, new Vector3(0, 0, 0), Quaternion.identity, parent.transform);
                Transform child = button.transform.Find("IconAndText");
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
                    attachListener(pressState);
                }
            }
            /*Returns the instance of pressState to caller of this fake constructor, used to check
            whether this button has been pressed or not.
            */
            return (pressState);
        }
    }
}
