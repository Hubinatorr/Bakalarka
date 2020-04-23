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

    public GameObject WayPointPointerPrefab;

    private float groundAltitude;

    void Start()
    {
        int i = 0;
        path = Application.streamingAssetsPath + "/mission.json";
        jsonContent =File.ReadAllText(path);
        Debug.Log(jsonContent);
        Mission mission = JsonUtility.FromJson<Mission>(jsonContent);
        // Iterate the points and spawn points
        foreach(var item in mission.checkpoints){
            if(item.type == "regular"){
                GameObject WayPointPointerPrefab;
                WayPointPointerPrefab = Resources.Load<GameObject>("Zones/WayPointPointer");
                GameObject WayPointPointer = Instantiate(WayPointPointerPrefab);
                WayPointPointer.transform.SetParent(transform);
                    // Vytvor vektor z gps
                Mapbox.Utils.Vector2d p = new Mapbox.Utils.Vector2d(item.points[0].latitude,item.points[0].longitude);
                    // // Ziskaj poziciu
                Vector3 position = Map.GeoToWorldPosition(p,false);
                Debug.Log(position);
                    // Ziskaj vysku
                groundAltitude = Map.QueryElevationInUnityUnitsAt(Map.WorldToGeoPosition(position));
                position.y = groundAltitude + (float)item.points[0].height;
                WayPointPointer.transform.position = position;
                item.points[0].point = WayPointPointer;
            } else if(item.type == "zone"){
                // Vytvor hranice zony
                foreach(var point in item.points){
                    GameObject WayPointPointerPrefab;
                    WayPointPointerPrefab = Resources.Load<GameObject>("Zones/ZoneWall");
                    GameObject WayPointPointer = Instantiate(WayPointPointerPrefab);
                    WayPointPointer.transform.SetParent(transform);
                        // Vytvor vektor z gps
                    Mapbox.Utils.Vector2d p = new Mapbox.Utils.Vector2d(point.latitude,point.longitude);
                        // // Ziskaj poziciu
                    Vector3 position = Map.GeoToWorldPosition(p,false);
                    Debug.Log(position);
                        // Ziskaj vysku
                    groundAltitude = Map.QueryElevationInUnityUnitsAt(Map.WorldToGeoPosition(position));
                    // Polovica pretoze sa scaluje zo stredu
                    position.y = groundAltitude + ((float)point.height/2);
                    
                    WayPointPointer.transform.localScale = new Vector3(1,(float)point.height,1); 
                    WayPointPointer.transform.position = position;
                    item.points[i].point = WayPointPointer;
                    i++;
                }

                i= 0;
                // Natiahni hranice zony
                foreach(var current in item.points){
                    float distance;
                    if(i == item.points.Count -1){
                        distance = Vector3.Distance(current.point.transform.position,item.points[0].point.transform.position);
                        current.point.transform.LookAt(item.points[0].point.transform);
                        current.point.transform.localScale = new Vector3(current.point.transform.localScale.x,current.point.transform.localScale.y,distance);
                    }else{
                        distance = Vector3.Distance(current.point.transform.position,item.points[i+1].point.transform.position);
                        current.point.transform.LookAt(item.points[i+1].point.transform);
                        current.point.transform.localScale = new Vector3(current.point.transform.localScale.x,current.point.transform.localScale.y,distance);
                    }
                    i++;    
                }
                i =0;
                foreach(var current in item.points){
                    float distance;
                    if(i == item.points.Count -1){
                        distance = Vector3.Distance(current.point.transform.position,item.points[0].point.transform.position);
                        current.point.transform.position = current.point.transform.position + distance/2 * current.point.transform.forward;
                    }else{
                        distance = Vector3.Distance(current.point.transform.position,item.points[i+1].point.transform.position);
                        current.point.transform.position = current.point.transform.position + distance/2 * current.point.transform.forward;
                    }
                    i++;    
                }
                i=0;
            }
        }
        Debug.Log(i);
    }

}

[System.Serializable]
public class Mission{
    public List<Checkpoint> checkpoints;
}
[System.Serializable]
public class Checkpoint
{
    public string type;
    public string name;
    public List <Point> points;
}

[System.Serializable]
public class Point{
    public GameObject point;
    public double height;
    public double latitude;
    public double longitude;
    
}