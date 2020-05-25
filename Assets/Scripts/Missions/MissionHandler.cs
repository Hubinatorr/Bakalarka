using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Unity.Map;
public class MissionHandler : MonoBehaviour
{
    public AbstractMap Map;
    private string path;
    private string jsonContent;
    public GameObject newDrone;
    public Transform ourDrone;
    private int droneNumber = 1;
    public Transform tarfetTransform;
    public GameObject dronesPrefab;

    public Transform iconTransform;
    public GameObject icon;
    public GameObject WayPointPointerPrefab;
    public GameObject PopUp;
    public RenderTexture PopUpRenderTexture;
    private float groundAltitude;

    public float speed;
    public GameObject arrow;

    public GameObject homepad;
    public Mission mission;

    public List<int> droneIndexes;

    public Text description;

    public Transform selectorParent;
    public GameObject defaultDrone;


    // Defaultly the first drone is 
    private int activeDrone = 0;
    public void AddDrone(Vector3 position){ 
        GameObject Clone = Instantiate(newDrone, position, ourDrone.rotation);
        Clone.name = "DroneObject" + droneNumber.ToString();
        Drones.drones.Add(Clone);
        Drones.DroneAdded(tarfetTransform,dronesPrefab,iconTransform,icon,PopUp, PopUpRenderTexture);
        droneNumber++;
        Clone.transform.SetParent(transform);
    }

    private void FillIndexes(){
        for(int f = 0 ; f < mission.drones.Count; f++){
            droneIndexes.Add(f);
        }
    }
    
    private void LoadJsonData(){
        path = Application.streamingAssetsPath + "/mission.json";
        jsonContent =File.ReadAllText(path);
        mission = JsonUtility.FromJson<Mission>(jsonContent);
    }

    void SetupDrones(){
        int i = 0;
        foreach(var item in mission.drones){
            Vector3 position3d;
            Mapbox.Utils.Vector2d mapboxPosition = new Mapbox.Utils.Vector2d(item.latitude,item.longitude);
            position3d = Map.GeoToWorldPosition(mapboxPosition,false);
            groundAltitude = Map.QueryElevationInUnityUnitsAt(Map.WorldToGeoPosition(position3d));
            position3d.y = groundAltitude;
            // Defaultne je prvy dron ten co uz je v scene
            if(i != activeDrone)
                AddDrone(position3d);
            i++;
        }        
    }

    private void GeneratePoint(Checkpoint item){
        GameObject WayPointPointerPrefab;
        WayPointPointerPrefab = Resources.Load<GameObject>("Zones/WayPointPointer");
        GameObject WayPointPointer = Instantiate(WayPointPointerPrefab);
        WayPointPointer.transform.SetParent(transform);
                    // Vytvor vektor z gps
        Mapbox.Utils.Vector2d p = new Mapbox.Utils.Vector2d(item.points[0].latitude,item.points[0].longitude);
                    // // Ziskaj poziciu
        Vector3 position = Map.GeoToWorldPosition(p,false);
                    // Ziskaj vysku
        groundAltitude = Map.QueryElevationInUnityUnitsAt(Map.WorldToGeoPosition(position));
        position.y = groundAltitude + (float)item.points[0].height;
        WayPointPointer.transform.position = position;
        WayPointPointer.name = item.name;
        item.points[0].pointGameObject = WayPointPointer;
        foreach(var name in item.drones){
            foreach(var drone in mission.drones){
                if(drone.name == name){
                    drone.checkpoints.Add(item);
                }
            }
        }
    }


    void Start()
    {
        
        int i = 0;
        LoadJsonData();
        FillIndexes();
        // Nastavime drony
        SetupDrones();

        i = 0;
        foreach(var checkpoint in mission.checkpoints){
            checkpoint.droneCount = checkpoint.drones.Count;
            // Drony dostanu misiu
            if(checkpoint.type == "regular"){
                GeneratePoint(checkpoint);
            } else if(checkpoint.type == "zone"){
                // Vytvor hranice zony
                GameObject tmp = new GameObject(checkpoint.name);
                tmp.transform.position = new Vector3(0,0,0);
                tmp.transform.SetParent(transform);
                foreach(var point in checkpoint.points){
                    GameObject WayPointPointerPrefab;
                    WayPointPointerPrefab = Resources.Load<GameObject>("Zones/ZoneWall");
                    
                    GameObject WayPointPointer = Instantiate(WayPointPointerPrefab);
                    
                    WayPointPointer.transform.SetParent(tmp.transform);
                        // Vytvor vektor z gps
                    Mapbox.Utils.Vector2d p = new Mapbox.Utils.Vector2d(point.latitude,point.longitude);
                        // // Ziskaj poziciu
                    Vector3 position = Map.GeoToWorldPosition(p,false);
                        // Ziskaj vysku
                    groundAltitude = Map.QueryElevationInUnityUnitsAt(Map.WorldToGeoPosition(position));
                    // Polovica pretoze sa scaluje zo stredu
                    position.y = groundAltitude + ((float)point.height/2);
                    
                    WayPointPointer.transform.localScale = new Vector3(1,(float)point.height,1); 
                    WayPointPointer.transform.position = position;
                    checkpoint.points[i].pointGameObject = WayPointPointer;
                    i++;
                }

                i= 0;
                float y = 0;
                // Natiahni hranice zony
                foreach(var current in checkpoint.points){
                    current.pointGameObject.layer = 13;
                    if(i == 0)
                        y = current.pointGameObject.transform.position.y;

                    current.pointGameObject.transform.position = new Vector3(current.pointGameObject.transform.position.x,y,current.pointGameObject.transform.position.z);
                    float distance;
                    if(i == checkpoint.points.Count -1){
                        distance = Vector3.Distance(current.pointGameObject.transform.position,checkpoint.points[0].pointGameObject.transform.position);
                        current.pointGameObject.transform.LookAt(checkpoint.points[0].pointGameObject.transform);
                        current.pointGameObject.transform.localScale = new Vector3(current.pointGameObject.transform.localScale.x,current.pointGameObject.transform.localScale.y,distance);
                    }else{
                        distance = Vector3.Distance(current.pointGameObject.transform.position,checkpoint.points[i+1].pointGameObject.transform.position);
                        current.pointGameObject.transform.LookAt(checkpoint.points[i+1].pointGameObject.transform);
                        current.pointGameObject.transform.localScale = new Vector3(current.pointGameObject.transform.localScale.x,current.pointGameObject.transform.localScale.y,distance);
                    }

                    i++;   
                    current.pointGameObject.transform.eulerAngles = new Vector3(
                        0,
                        current.pointGameObject.transform.eulerAngles.y,
                        0
                    );


                }
                i =0;
                foreach(var current in checkpoint.points){
                    float distance;
                    if(i == checkpoint.points.Count -1){
                        distance = Vector3.Distance(current.pointGameObject.transform.position,checkpoint.points[0].pointGameObject.transform.position);
                        current.pointGameObject.transform.position = current.pointGameObject.transform.position + distance/2 * current.pointGameObject.transform.forward;
                    }else{
                        distance = Vector3.Distance(current.pointGameObject.transform.position,checkpoint.points[i+1].pointGameObject.transform.position);
                        current.pointGameObject.transform.position = current.pointGameObject.transform.position + distance/2 * current.pointGameObject.transform.forward;
                    }
                    i++;    
                }
                i=0;
                // Dronovi priradime misiu
                foreach(var name in checkpoint.drones){
                    foreach(var drone in mission.drones){
                        if(drone.name == name){
                            drone.checkpoints.Add(checkpoint);
                        }
                    }
                }


                // Centrum zony pre navigaciu
                GameObject PointerPrefab;
                PointerPrefab = Resources.Load<GameObject>("Zones/ZoneWall");
                GameObject Middle = Instantiate(PointerPrefab);
                Vector3 center = new Vector3(0, 0, 0);
                float count = 0;
                foreach (var pointOfZone in checkpoint.points){
                    center += pointOfZone.pointGameObject.transform.position;
                    count++;
                }
                var theCenter = center / count;
                Middle.transform.position = theCenter;
                Middle.name = checkpoint.name;
                Middle.transform.SetParent(tmp.transform);
                Point centerPoint = new Point();
                centerPoint.pointGameObject = Middle;
                checkpoint.points.Add(centerPoint);
            }
        }
    }
    public float offset = 1.0F;
    public float zoneOffset = 100.0F;
    void Update(){
        if(mission.drones[0].checkpoints.Count > 0)
            if(mission.drones[0].checkpoints[0].type == "regular")
                homepad.transform.position = mission.drones[0].checkpoints[0].points[0].pointGameObject.transform.position;
            else if(mission.drones[0].checkpoints[0].type == "zone")
                homepad.transform.position = mission.drones[0].checkpoints[0].points[mission.drones[0].checkpoints[0].points.Count-1].pointGameObject.transform.position;
        int i = 0;
        foreach(Transform child in tarfetTransform.transform)
                {
                    Text height = child.Find("Height").GetComponent<Text>();
                    Text objective = child.Find("Objective").GetComponent<Text>();
                    float droneAltitude = 0.0f;
                    droneAltitude = Drones.drones[i].transform.localPosition.y - Map.QueryElevationInUnityUnitsAt(Map.WorldToGeoPosition(Drones.drones[i].transform.position));
                    height.text = "Height:" + Mathf.Round(droneAltitude) + "m";
                    objective.text = "Objective: " + mission.drones[i].checkpoints[0].name;
                    i++;
                }
                i=0;

        foreach(var drone in mission.drones){
            // Prvy dron, ten ktory ovladas
            if(i == 0){
                if(drone.checkpoints.Count > 0){
                    // zisti ci je to zona alebo point
                    if(drone.checkpoints[0].type == "regular"){
                        // Vzdialenost dronu od pointu v jeho liste
                        if(Vector3.Distance(Drones.drones[i].transform.position, drone.checkpoints[0].points[0].pointGameObject.transform.position) > offset){
                            // Chod k pointu

                            foreach(Transform child in selectorParent){
                                child.GetComponent<Image>().color = new Color(0.3301887F,0.3301887F,0.3301887F,0.6666667F);
                            }
                           description.text = "Reach "+  drone.checkpoints[0].name;
                        } else {
                            // Je uz tam
                            description.text = "Wait for other drones";
                            if(droneIndexes.Contains(i)){
                                droneIndexes.Remove(i);
                                // Odcitam ten index
                                mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount--;
                                Debug.Log(mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount);
                            }
                            int j = 0;
                                foreach(Transform child in selectorParent){
                                    if(droneIndexes.Contains(j) && drone.checkpoints[0].drones.Contains(mission.drones[j].name)){
                                        child.GetComponent<Image>().color = new Color(0.5660378F,0.06674973F,0.06674973F,6666667F);
                                    } else {
                                        child.GetComponent<Image>().color = new Color(0.3301887F,0.3301887F,0.3301887F,0.6666667F);
                                    }
                                    j++;
                                }

                            if(mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount == 0){
                                droneIndexes.Add(i);
                                drone.checkpoints.RemoveAt(0);
                            }
                        }
                    } else if(drone.checkpoints[0].type == "zone"){
                        
                        // Posledny prvok je stred zony
                        int last_index = drone.checkpoints[0].points.Count-1;
                        if(Vector3.Distance(Drones.drones[i].transform.position, drone.checkpoints[0].points[last_index].pointGameObject.transform.position) > zoneOffset){
                            // Chod k stredu zony
                            foreach(Transform child in selectorParent){
                                child.GetComponent<Image>().color = new Color(0.3301887F,0.3301887F,0.3301887F,0.6666667F);
                            }
                            description.text = "Reach "+  drone.checkpoints[0].name;
                        } else {
                            // Je uz tam a este sa neodciatal
                            description.text = "Wait for other drones";
                            if(droneIndexes.Contains(i)){
                                droneIndexes.Remove(i);
                                mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount--;
                                Debug.Log(mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount);
                            }
                            int j = 0;
                                foreach(Transform child in selectorParent){
                                    if(droneIndexes.Contains(j) && drone.checkpoints[0].drones.Contains(mission.drones[j].name)){
                                        child.GetComponent<Image>().color = new Color(0.5660378F,0.06674973F,0.06674973F,6666667F);
                                    } else {
                                        child.GetComponent<Image>().color = new Color(0.3301887F,0.3301887F,0.3301887F,0.6666667F);
                                    }
                                    j++;
                                }

                            if(mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount == 0){
                                droneIndexes.Add(i);
                                var zone = mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).points;
                                foreach(var point in zone){
                                    Color green = new Color(0.03921568F,0.9333333F,0.1818267F,0.5882353F);
                                    point.pointGameObject.GetComponent<Renderer>().material.SetColor("_Color",green);
                                }
                                drone.checkpoints.RemoveAt(0);
                            }
                        }
                    }
                } 

                // if(Vector3.Distance(Drones.drones[i].transform.position, drone.checkpoints[0].points[0].pointGameObject.transform.position) < offset){
                //     if(droneIndexes.Contains(i)){
                //         droneIndexes.Remove(i);
                //         // Odcitam ten index
                //         mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount--;
                //         Debug.Log(mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount);
                //     }
                //     if(mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount == 0){
                //         droneIndexes.Add(i);
                //         drone.checkpoints.RemoveAt(0);
                //     }
                // }

            } else { // Ostatne drony
                // Ak ma este misiu
                if(drone.checkpoints.Count > 0){
                    // zisti ci je to zona alebo point
                    if(drone.checkpoints[0].type == "regular"){
                        // Vzdialenost dronu od pointu v jeho liste
                        if(Vector3.Distance(Drones.drones[i].transform.position, drone.checkpoints[0].points[0].pointGameObject.transform.position) > offset){
                            // Chod k pointu
                            Drones.drones[i].transform.position = Vector3.MoveTowards(Drones.drones[i].transform.position, drone.checkpoints[0].points[0].pointGameObject.transform.position, Time.deltaTime * speed);
                        } else {
                            // Je uz tam
                            if(droneIndexes.Contains(i)){
                                droneIndexes.Remove(i);
                                // Odcitam ten index
                                mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount--;
                                Debug.Log(mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount);
                            }

                            if(mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount == 0){
                                droneIndexes.Add(i);
                                drone.checkpoints.RemoveAt(0);
                            }
                        }
                    } else if(drone.checkpoints[0].type == "zone"){
                        // Posledny prvok je stred zony
                        int last_index = drone.checkpoints[0].points.Count-1;
                        if(Vector3.Distance(Drones.drones[i].transform.position, drone.checkpoints[0].points[last_index].pointGameObject.transform.position) > zoneOffset){
                            // Chod k stredu zony
                            Drones.drones[i].transform.position = Vector3.MoveTowards(Drones.drones[i].transform.position, drone.checkpoints[0].points[last_index].pointGameObject.transform.position, Time.deltaTime * speed);
                        } else {
                            // Je uz tam a este sa neodciatal
                            if(droneIndexes.Contains(i)){
                                droneIndexes.Remove(i);
                                mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount--;
                                Debug.Log(mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount);
                            }

                            if(mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount == 0){
                                droneIndexes.Add(i);
                                var zone = mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).points;
                                foreach(var point in zone){
                                     Color green = new Color(0.03921568F,0.9333333F,0.1818267F,0.5882353F);
                                    point.pointGameObject.GetComponent<Renderer>().material.SetColor("_Color",green);
                                }
                                drone.checkpoints.RemoveAt(0);
                            }
                        }
                    }
                } 
            }
            i++;
        }
    }

}



[System.Serializable]
public class Mission{
    public List<Drone> drones;
    public List<Checkpoint> checkpoints;
}
[System.Serializable]
public class Checkpoint
{
    public string type;
    public string name;
    public List <Point> points;

    public List <string> drones;

    public int droneCount;
}

[System.Serializable]
public class Point{
    public GameObject pointGameObject;
    public double height;
    public double latitude;
    public double longitude;    
}

[System.Serializable]
public class Drone{
    public string name;
    public double latitude;
    public double longitude;
    public List <Checkpoint> checkpoints;
}