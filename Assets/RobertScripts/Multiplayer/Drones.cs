using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Drones : MonoBehaviour
{
    public Transform Camera;
    public static List<GameObject> drones = new List<GameObject>();
    public Transform tarfetTransform;
    public GameObject dronesPrefab;
    private Text[] droneName;

    float distance = 0;

    private GameObject droneBody;
    private GameObject activeDrone;
    private int i = 0;
    // Start is called before the first frame update
    
    void Start()
    {
        foreach(GameObject item in drones)
        {
            GameObject droneDisplay = (GameObject)Instantiate(dronesPrefab);
            droneDisplay.transform.SetParent(tarfetTransform);
            droneName = droneDisplay.GetComponentsInChildren<Text>();
            droneName[0].text = item.name;
            droneName[1].text = drones.Count.ToString();
        }
    }


    // Update is called once per frame
    void Update()
    {
        foreach(Transform child in tarfetTransform.transform)
        {
            droneName = child.GetComponentsInChildren<Text>();
            // distance = Vector3.Distance(drones[0].transform.position, drones[0].transform.position);
            // droneName[1].text = Mathf.Round(distance) + "m";
            distance = Vector3.Distance(drones[0].transform.position,drones[i].transform.position);
            droneName[1].text = Mathf.Round(distance) + "m";
            i++;
        }

        i=0;
    }

    public static void DroneAdded(Transform tarfetTransform, GameObject dronesPrefab)
    {
        GameObject item = drones[drones.Count - 1];
        GameObject droneDisplay = (GameObject)Instantiate(dronesPrefab);
        droneDisplay.transform.SetParent(tarfetTransform);
        Text[] droneName = droneDisplay.GetComponentsInChildren<Text>();
        droneName[0].text = item.name;
        droneName[1].text = "0m";
        float distance = Vector3.Distance(drones[0].transform.position, item.transform.position);
        Debug.Log(distance);   
    }

    public static void LookFunction(int i){
        Transform Camera = drones[0].transform.Find("CameraBox/MainCameraPair");
        Debug.Log(Camera.gameObject.name);
        Camera.LookAt(Drones.drones[i].transform);
    }
}
