using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LookAtDrone : MonoBehaviour
{
    private Text[] droneName;
    private Transform defaultView;



    public GameObject PopUp;

    public RenderTexture RenderTexture; 
    int i;
    public void LookDrone(){
        droneName = transform.GetComponentsInChildren<Text>();
        i = Int32.Parse(droneName[0].text.Substring(droneName[0].text.Length -1));
        // Camera.LookAt(Drones.drones[i].transform);
        PopUp.SetActive(true);
        Camera PopUpCamera;
        PopUpCamera = Drones.drones[i].transform.Find("TopCamera").GetComponent<Camera>();
        PopUpCamera.gameObject.SetActive(true);
        PopUpCamera.targetTexture = RenderTexture;
        PopUpCamera.enabled = true;
        Debug.Log(RenderTexture);
        Debug.Log(PopUpCamera);
    }

    public void DontLookDrone(){
        droneName = transform.GetComponentsInChildren<Text>();
        i = Int32.Parse(droneName[0].text.Substring(droneName[0].text.Length -1));
         Camera PopUpCamera;
        PopUpCamera = Drones.drones[i].transform.Find("TopCamera").GetComponent<Camera>();
        PopUpCamera.gameObject.SetActive(false);
        PopUp.SetActive(false);
    }


}
