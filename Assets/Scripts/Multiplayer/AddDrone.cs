using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddDrone : MonoBehaviour
{
    public GameObject newDrone;
    public Transform ourDrone;
    private int droneNumber = 1;
    public Transform tarfetTransform;
    public GameObject dronesPrefab;

    public Transform iconTransform;
    public GameObject icon;

    public void addDrone(){
        Vector3 newPosition = ourDrone.position;
        newPosition.x += 5;
        newPosition.y += 5;
        newPosition.z += 5;

        GameObject Clone = Instantiate(newDrone, newPosition, ourDrone.rotation);
        Clone.name = "DroneObject" + droneNumber.ToString();
        //DroneList.drones.Add(Clone);ß
        //DisplayDrones.dronesList.Add(Clone);
        Drones.drones.Add(Clone);
        Drones.DroneAdded(tarfetTransform,dronesPrefab,iconTransform,icon);
        Debug.Log(Clone.name + "added");
        droneNumber++;
        //Clone.GetComponent("DroneController").enabled = false;
    }
}
  