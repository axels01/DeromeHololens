using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneObject : MonoBehaviour
{

    public GameObject model;

    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        //model.SetActive(false);
    }

    public void showModel()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        Vector3 playerPos = Camera.main.transform.position;
        Vector3 spawnPos = model.transform.position;
        Debug.Log(Camera.main.transform.forward);
        spawnPos.x = playerPos.x + Camera.main.transform.forward.x * 1;
        spawnPos.z = playerPos.z + Camera.main.transform.forward.z * 1;
        spawnPos.y += 0.935f;
        model.transform.position = spawnPos;
        model.SetActive(true);
    }
}
