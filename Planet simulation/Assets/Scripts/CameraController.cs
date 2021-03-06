﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public float mainSpeed = 100.0f; //regular speed
    float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
    float camSens = 0.25f; //How sensitive it with mouse
    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;
    public static EventHandler speedChanged;
    public static EventHandler uiToggled;

    private void Start ()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update ()
    {


        lastMouse = new Vector3(transform.eulerAngles.x - Input.GetAxis("Mouse Y"), transform.eulerAngles.y + Input.GetAxis("Mouse X"), 0);
        transform.eulerAngles = lastMouse;

        //Mouse  camera angle done.  



        //Keyboard commands
        Vector3 p = GetBaseInput();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            totalRun += Time.deltaTime;
            p = p * totalRun * shiftAdd;
        }
        else
        {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            p = p * mainSpeed;
        }

        p = p * Time.deltaTime;
        Vector3 newPosition = transform.position;
        if (Input.GetKey(KeyCode.Space))
        { //If player wants to move on X and Z axis only
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else
        {
            transform.Translate(p);
        }

    }

    private Vector3 GetBaseInput ()
    { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            Quit();
        }
        if (Input.GetKey(KeyCode.E))
        {
            p_Velocity += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            p_Velocity += new Vector3(0, -1, 0);
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            mainSpeed += mainSpeed * 0.25f;
            shiftAdd = mainSpeed * 2;
            OnSpeedChanged();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            mainSpeed -= mainSpeed * 0.25f;
            shiftAdd = mainSpeed * 2;
            OnSpeedChanged();
        }
        if (Input.GetKeyDown(KeyCode.U))
            OnUIToggled();


        return p_Velocity;
    }

    protected virtual void OnSpeedChanged ()
    {
        speedChanged(this, new EventArgs());
    }

    protected virtual void OnUIToggled ()
    {
        uiToggled(this, new EventArgs());
    }

    public static void Quit ()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
         Application.OpenURL(webplayerQuitURL);
#else
         Application.Quit();
#endif
    }
}
