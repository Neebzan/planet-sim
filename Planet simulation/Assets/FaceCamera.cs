using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FaceCamera : MonoBehaviour
{
    void Start ()
    {
        Text text = GetComponentInChildren<Text>();
        text.transform.localScale = new Vector3(-1, text.transform.localScale.y, transform.localScale.z);
    }

    void FixedUpdate()
    {
        transform.LookAt(Camera.main.transform);
    }
}
