using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disapearonclick : MonoBehaviour
{
    void Update()
    {
        // MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        //Material material = meshRenderer.material;
        GameObject cube = GameObject.FindGameObjectWithTag("cube");
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            cube.SetActive(false);
        }
    }
}
