﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using UnityEngine.UI;
using System.IO;
using RosSharp.RosBridgeClient;
using TMPro;

//vygeneruju budovy
/*
for (int i = 0; i < 150; i++)
{
    GameObject newBuilding = Instantiate(building, new Vector3(Random.Range(-250f, 250f), 1.0f, Random.Range(-250f, 250f)), Quaternion.Euler(0, Random.Range(-180f, 180f), 0));
    newBuilding.transform.localScale = new Vector3(Random.Range(1.0f, 20.0f), Random.Range(2.0f, 10.0f),Random.Range(5.0f, 20.0f));
}
*/

public class DroneController : MonoBehaviour {
    private AbstractDroneData positionData, positionDataM, positionDataS;
    private DroneRosData positionDataR; //ma specialni metodu pro nastaveni rosConnectoru
    public GameObject droneModel;
    private GameObject rosConnector;
    public GameObject rosConnectorPrefab;
    public GameObject droneCamera; //model kamery
    public AbstractMap Map;
    public TextMeshProUGUI heightText;
    public TextMeshProUGUI ConnectorText;

    public GameObject videoObjects;
    public GameObject videoScreen;
    public GameObject videoProjector;

    public bool isProjectorActive = false;
    public bool isVideoScreenActive = true;

    private int dataSource=1;
    /* 0 simulovaný vstup - náhodný pohyb
     * 1 manuální řízení ovladačem
     * 2 ROS
     */

    // Use this for initialization
    void Start() {
        // Prvy dron je vzdy ten defaultny
        string path = Application.streamingAssetsPath + "/mission.json";
        string jsonContent =File.ReadAllText(path);

        Mission mission = JsonUtility.FromJson<Mission>(jsonContent);
        Vector3 pos;
        Mapbox.Utils.Vector2d p = new Mapbox.Utils.Vector2d(mission.drones[0].latitude,mission.drones[0].longitude);
        pos = Map.GeoToWorldPosition(p,false);
        Debug.Log(pos);
        float groundAltitude = Map.QueryElevationInUnityUnitsAt(Map.WorldToGeoPosition(pos));
        pos.y = groundAltitude;
        Debug.Log("thr" + pos);


        positionDataS = new DroneData(Map, pos);
        positionData = positionDataM = new DroneDataManual(Map, pos);
        positionDataR = new DroneRosData(Map, pos);
        Drones.drones.Add(transform.gameObject);

    }

    public void ConnectToRos()  // může být voláno z GUI
    {
        GameObject oldRosConnector = rosConnector;

        rosConnector = Instantiate(rosConnectorPrefab);
        positionDataR.setRosConnector(rosConnector); // je potřeba aby si zaktualizoval odkazy na komponenty

        if (oldRosConnector != null)
        {
            oldRosConnector.GetComponent<RosConnector>().Disconnect();
            Destroy(oldRosConnector);
        }
    }

    public void SwitchVideoScreen() //pokud je screen aktivni, tak ho vypne a naopak
    {
        videoScreen.SetActive(!videoScreen.activeSelf);

        if (videoScreen.activeSelf && videoProjector.activeSelf) //deaktivuju simulovanym kliknutim
            SwitchProjector();

        isProjectorActive = videoProjector.activeSelf;
        isVideoScreenActive = videoScreen.activeSelf;
    }


    public void SwitchProjector() //pokud je projektor aktivni, tak ho vypne a naopak
    {
        videoProjector.SetActive(!videoProjector.activeSelf);


        if (videoScreen.activeSelf && videoProjector.activeSelf) //deaktivuju simulovanym kliknutim
            SwitchVideoScreen();

        isProjectorActive = videoProjector.activeSelf;
        isVideoScreenActive = videoScreen.activeSelf;
    }


    /* 0 simulovaný vstup - náhodný pohyb
    * 1 manuální řízení ovladačem
     * 2 ROS
    */

    public int changeDataSource(int source = -1)
    {
        if (source == -1)
            source = (dataSource + 1) % 3;

        if (source == 0 && dataSource != 0)
        {
            Vector3 pos = positionData.GetPosition();
            Vector3 rot = positionData.GetRotation();
            dataSource = 0;
            Debug.Log("přepnuto na simulovany vstup - nahodna data");
            positionData = positionDataS;
            positionData.reset(pos, rot);//reset rychlosti akcelerace
        }

        if (source==1 && dataSource != 1)
        {
            Vector3 pos = positionData.GetPosition();
            Vector3 rot = positionData.GetRotation();
            dataSource = 1;
            Debug.Log("přepnuto na manuál");
            positionData = positionDataM;
            positionData.reset(pos, rot);
        }

        if (source == 2 && dataSource != 2)
        {
            Vector3 pos = positionData.GetPosition();
            Vector3 rot = positionData.GetRotation();
            if (rosConnector == null)
            {
                ConnectToRos();
            }
            positionDataR.setRosConnector(rosConnector);
            dataSource = 2;
            Debug.Log("přepnuto vstup z ros");
            positionData = positionDataR;
            // positionData.reset(pos,rot); //reset rychlosti akcelerace
        }

        return dataSource;
    }

    public int getDataSource()
    {
        return dataSource;
    }

        // Update is called once per frame
        void Update()
    {
         positionData.update();

        transform.localPosition= positionData.GetPosition();
        transform.localRotation = Quaternion.Euler(positionData.GetRotation());

        float groundAltitude = positionData.getGroundAltitude();

        float droneAltitude = transform.localPosition.y;
        //Debug.Log("ground: " + groundAltitude +  drone: "+ droneAltitude + "m, height: "+ (droneAltitude- groundAltitude) + "m, distance: " + positionData.GetHomeDistance()+"m");

        heightText.text = "Height: " + droneAltitude.ToString("F1") + "m a.s.l.     Ground height: " + (droneAltitude - groundAltitude).ToString("F1") + "m     Horizonal distance: " + positionData.GetHomeDistance().ToString("F1") + "m";

        videoObjects.transform.localRotation = Quaternion.Euler(positionData.GetCameraRotation());
        droneCamera.transform.localRotation = Quaternion.Euler(positionData.GetCameraRotation());

        droneModel.transform.localRotation = Quaternion.Euler(positionData.GetPitchRoll() + new Vector3(0, 90, 0));

        //vypíšeme hlášku o připojení k ROSbridge
        if (rosConnector != null)
        {
            int state = rosConnector.GetComponent<MyRosConnector>().ConnectionState;
            if (state==1)
                ConnectorText.text = "Connected";
            else if (state == 2)
                ConnectorText.text = "Problematic";
            else if (state == 3)
                ConnectorText.text = "Cennection lost" +
                    "" +
                    "" +
                    "" +
                    "";
            else if (state == 0)
                ConnectorText.text = "Connecting...";
            else ConnectorText.text = "Disonnected";
        }


        if (rosConnector != null) rosConnector.GetComponent<IDroneImageSubscriber>().OptimalizeForProjector = isProjectorActive;
    }
}
