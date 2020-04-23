using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LookAtDrone : MonoBehaviour
{
    private Text[] droneName;
    private Transform defaultView;
    int i;
    public void LookDrone(){
        droneName = transform.GetComponentsInChildren<Text>();
        i = Int32.Parse(droneName[0].text.Substring(droneName[0].text.Length -1));
        Drones.LookFunction(i);
        // Camera.LookAt(Drones.drones[i].transform);
    }

    public void DontLookDrone(){
        droneName = transform.GetComponentsInChildren<Text>();
        i = Int32.Parse(droneName[0].text.Substring(droneName[0].text.Length -1));
        Drones.DontLookFunction(i);
    }


}
