using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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

    public GameObject defaultDrone;
    public void addDrone(Vector3 position){ 
        Vector3 newPosition =position;

        GameObject Clone = Instantiate(newDrone, newPosition, ourDrone.rotation);
        Clone.name = "DroneObject" + droneNumber.ToString();
        Drones.drones.Add(Clone);
        Drones.DroneAdded(tarfetTransform,dronesPrefab,iconTransform,icon,PopUp, PopUpRenderTexture);
        droneNumber++;
        Clone.transform.SetParent(transform);
        //Clone.GetComponent("DroneController").enabled = false;
    }

    void fill(){
        for(int f = 0 ; f < mission.drones.Count; f++){
            droneIndexes.Add(f);
        }
    }

    void Start()
    {
        
        int i = 0;
        path = Application.streamingAssetsPath + "/mission.json";
        jsonContent =File.ReadAllText(path);

        mission = JsonUtility.FromJson<Mission>(jsonContent);
        // Iterate the points and spawn points

        // List na overenie podmienky v update
        fill();
        // Nastavime drony
        foreach(var item in mission.drones){
            Vector3 pos;
            Mapbox.Utils.Vector2d p = new Mapbox.Utils.Vector2d(item.latitude,item.longitude);
            pos = Map.GeoToWorldPosition(p,false);
            groundAltitude = Map.QueryElevationInUnityUnitsAt(Map.WorldToGeoPosition(pos));
            pos.y = groundAltitude;
            
            // Defaultne je prvy dron ten co uz je v scene
            if(i == 0){
                
            } else {
                addDrone(pos);
            }
            
            i++;
        }
        i = 0;
        foreach(var item in mission.checkpoints){
            item.droneCount = item.drones.Count;
            // Drony dostanu misiu
            if(item.type == "regular"){
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
            } else if(item.type == "zone"){
                // Vytvor hranice zony
                GameObject tmp = new GameObject(item.name);
                tmp.transform.position = new Vector3(0,0,0);
                tmp.transform.SetParent(transform);
                foreach(var point in item.points){
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
                    item.points[i].pointGameObject = WayPointPointer;
                    i++;
                }

                i= 0;
                // Natiahni hranice zony
                foreach(var current in item.points){
                    float distance;
                    if(i == item.points.Count -1){
                        distance = Vector3.Distance(current.pointGameObject.transform.position,item.points[0].pointGameObject.transform.position);
                        current.pointGameObject.transform.LookAt(item.points[0].pointGameObject.transform);
                        current.pointGameObject.transform.localScale = new Vector3(current.pointGameObject.transform.localScale.x,current.pointGameObject.transform.localScale.y,distance);
                    }else{
                        distance = Vector3.Distance(current.pointGameObject.transform.position,item.points[i+1].pointGameObject.transform.position);
                        current.pointGameObject.transform.LookAt(item.points[i+1].pointGameObject.transform);
                        current.pointGameObject.transform.localScale = new Vector3(current.pointGameObject.transform.localScale.x,current.pointGameObject.transform.localScale.y,distance);
                    }
                    i++;    
                }
                i =0;
                foreach(var current in item.points){
                    float distance;
                    if(i == item.points.Count -1){
                        distance = Vector3.Distance(current.pointGameObject.transform.position,item.points[0].pointGameObject.transform.position);
                        current.pointGameObject.transform.position = current.pointGameObject.transform.position + distance/2 * current.pointGameObject.transform.forward;
                    }else{
                        distance = Vector3.Distance(current.pointGameObject.transform.position,item.points[i+1].pointGameObject.transform.position);
                        current.pointGameObject.transform.position = current.pointGameObject.transform.position + distance/2 * current.pointGameObject.transform.forward;
                    }
                    i++;    
                }
                i=0;
                // Dronovi priradime misiu
                foreach(var name in item.drones){
                    foreach(var drone in mission.drones){
                        if(drone.name == name){
                            drone.checkpoints.Add(item);
                        }
                    }
                }


                // Centrum zony pre navigaciu
                GameObject PointerPrefab;
                PointerPrefab = Resources.Load<GameObject>("Zones/ZoneWall");
                GameObject Middle = Instantiate(PointerPrefab);
                Vector3 center = new Vector3(0, 0, 0);
                float count = 0;
                foreach (var pointOfZone in item.points){
                    center += pointOfZone.pointGameObject.transform.position;
                    count++;
                }
                var theCenter = center / count;
                Middle.transform.position = theCenter;
                Middle.name = item.name;
                Middle.transform.SetParent(tmp.transform);
                Point centerPoint = new Point();
                centerPoint.pointGameObject = Middle;
                item.points.Add(centerPoint);
            }
        }
    }
    public float offset = 1.0F;

    void Update(){
        if(mission.drones[0].checkpoints.Count > 0)
            if(mission.drones[0].checkpoints[0].type == "regular")
                homepad.transform.position = mission.drones[0].checkpoints[0].points[0].pointGameObject.transform.position;
            else if(mission.drones[0].checkpoints[0].type == "zone")
                homepad.transform.position = mission.drones[0].checkpoints[0].points[mission.drones[0].checkpoints[0].points.Count-1].pointGameObject.transform.position;
        int i = 0;
        foreach(var drone in mission.drones){
            // Prvy dron, ten ktory ovladas
            if(i == 0){
                if(drone.checkpoints.Count > 0){
                    // zisti ci je to zona alebo point
                    if(drone.checkpoints[0].type == "regular"){
                        // Vzdialenost dronu od pointu v jeho liste
                        if(Vector3.Distance(Drones.drones[i].transform.position, drone.checkpoints[0].points[0].pointGameObject.transform.position) > offset){
                            // Chod k pointu
                           
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
                        if(Vector3.Distance(Drones.drones[i].transform.position, drone.checkpoints[0].points[last_index].pointGameObject.transform.position) > offset){
                            // Chod k stredu zony
                            
                        } else {
                            // Je uz tam a este sa neodciatal
                            if(droneIndexes.Contains(i)){
                                droneIndexes.Remove(i);
                                mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount--;
                                Debug.Log(mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount);
                            }

                            if(mission.checkpoints.Find(Point => Point.name == drone.checkpoints[0].name).droneCount == 0){
                                droneIndexes.Add(i);
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
                        if(Vector3.Distance(Drones.drones[i].transform.position, drone.checkpoints[0].points[last_index].pointGameObject.transform.position) > offset){
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