using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class relativetoImageTarget: MonoBehaviour
{
    public GameObject obj;
    public GameObject imageTarget;

    void Update()
    {
        obj.transform.position = imageTarget.transform.position;
        // Get your targets right vector in world space
        var right = imageTarget.transform.right;

        // If not anyway the case ensure that your objects up vector equals the world up vector
        obj.transform.up = Vector3.up;

        // Align your objects right vector with the image target's right vector
        // projected down onto the global XZ plane => erasing its Y component
        obj.transform.right = Vector3.ProjectOnPlane(right, Vector3.up);
    }
}

