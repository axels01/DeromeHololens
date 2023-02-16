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

        //Checker
        public bool pressed(PressState pressState)
        {
            bool result = pressState.isPressed;
            if (result)
                Debug.Log("BM is: " + result);
            pressState.reset();
            return result;
        }

        private void attachListener(PressState pressState)
        {
            Interactable interactable = button.GetComponent<Interactable>();
            interactable.OnClick.AddListener(() => Debug.Log("Interactable clicked"));
            if (interactable != null)
            {
                interactable.OnClick.AddListener(() => pressState.SetPressed());
            }
        }

        /*Issue with changing value of local variable from AddListener line thus the workaround
        *With using a fake constructor
        https://forum.unity.com/threads/onclick-addlistener-with-a-string-parameter.892210/
        Constructor, parent and name set to defaut null, used when only used to check on button presses
        and not creating a new button object from prefab*/
        public PressState buttonManagerBuilder(GameObject gameObject, GameObject parent = null, string name = null)
        {
            PressState pressState = new PressState();
            if (parent == null && name == null)
            {
                // Runs when first argument is an already created button, used when only "press checking" is needed
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

            return (pressState);
        }
    }
}
