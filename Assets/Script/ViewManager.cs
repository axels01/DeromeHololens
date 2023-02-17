using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ButtonManager;


namespace ViewManager
{
    public class UIButtons : MonoBehaviour
    {
        public Dictionary<GameObject, UIButton> uIButtons = new Dictionary<GameObject, UIButton>();

        public string update()
        {
            foreach(KeyValuePair<GameObject, UIButton> pair in uIButtons)
            {
                if (uIButtons[pair.Key].buttonManager.pressed(uIButtons[pair.Key].pressState))
                    return (pair.Value.action);
            }
            return null;
        }

        public void add(GameObject gameObject, string action)
        {
            UIButton tempUIButton = new UIButton();
            tempUIButton.action = action;
            tempUIButton.buttonManager = new buttonManager();
            tempUIButton.pressState = tempUIButton.buttonManager.buttonManagerBuilder(gameObject);
            uIButtons.Add(gameObject, tempUIButton);
        }
    }

    public class UIButton
    {
        public string action { get; set; }
        public buttonManager buttonManager { get; set; }
        public PressState pressState { get; set; }
    }
}
