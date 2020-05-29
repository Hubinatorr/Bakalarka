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

    Mesh mesh;
    Vector3[] vertices;
    int [] triangles;

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
    }


    private void GenerateZone(Checkpoint checkpoint){

        GameObject ZoneGameObject = new GameObject(checkpoint.name);
        ZoneGameObject.transform.position = new Vector3(0,0,0);
        ZoneGameObject.transform.SetParent(transform);
        ZoneGameObject.layer =13;
        ZoneGameObject.AddComponent<MeshFilter>();
        ZoneGameObject.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Zones/WayPointMaterial");
        List <Vector3> points = new List<Vector3>();

        // Minimalna groundAltitude aby zona nelevitovala
        float minAltitude = float.MaxValue;

        // Ziskam list bodov zony
        int j = 0;
        foreach(var point in checkpoint.points){
            GameObject WayPointPointer = Instantiate(Resources.Load<GameObject>("Zones/ZoneWall")); 
            WayPointPointer.transform.SetParent(ZoneGameObject.transform);

            // Vytvor vektor z gps
            Mapbox.Utils.Vector2d position2d = new Mapbox.Utils.Vector2d(point.latitude,point.longitude);
            // // Ziskaj poziciu
            Vector3 position = Map.GeoToWorldPosition(position2d,false);
            // Ziskaj vysku
            groundAltitude = Map.QueryElevationInUnityUnitsAt(Map.WorldToGeoPosition(position));
            if(groundAltitude < minAltitude)
                minAltitude= groundAltitude;
            position.y = groundAltitude + (float)point.height;
            points.Add(position);
            WayPointPointer.transform.position = position;
            checkpoint.points[j].pointGameObject = WayPointPointer;
            j++;
        }

        mesh = new Mesh();
        ZoneGameObject.GetComponent<MeshFilter>().mesh = mesh;
        // Pridaj samostatny prvok^^ TODO
        List<Vector3> verticesList = new List<Vector3>();
        List<int> trianglesList = new List<int>();
        int multiplier;

        // Ziskam pointy a trojuholniky z nich
        for(int i = 0; i < points.Count-1; i++){
            multiplier = i*4;
            
            verticesList.AddRange(new List<Vector3>(){
                points[i],
                points[i+1],
                new Vector3(points[i].x,minAltitude,points[i].z),
                new Vector3(points[i+1].x,minAltitude,points[i+1].z)
            });


            trianglesList.AddRange(new int[]{
                0+multiplier ,1+multiplier ,2+multiplier,
                0+multiplier ,2+multiplier ,1+multiplier,
                1+multiplier ,2+multiplier ,3+multiplier,
                1+multiplier ,3+multiplier ,2+multiplier
            });
        } 

        // Poslednu stenu treba spojit s prvou
        multiplier = (points.Count-1)*4;
        verticesList.AddRange(new List<Vector3>(){
            points[points.Count-1],
            points[0],
            new Vector3(points[points.Count-1].x,minAltitude,points[points.Count-1].z),
            new Vector3(points[0].x,minAltitude,points[0].z)
        });


        trianglesList.AddRange(new int[]{
            0+multiplier ,1+multiplier ,2+multiplier,
            0+multiplier ,2+multiplier ,1+multiplier,
            1+multiplier ,2+multiplier ,3+multiplier,
            1+multiplier ,3+multiplier ,2+multiplier
        });

        vertices = verticesList.ToArray();
        triangles = trianglesList.ToArray();
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;  
        
        GenerateMiddlePoint(checkpoint).transform.SetParent(ZoneGameObject.transform);
    }

    GameObject GenerateMiddlePoint(Checkpoint checkpoint){
        GameObject Middle = Instantiate(Resources.Load<GameObject>("Zones/ZoneWall"));
        Vector3 center = new Vector3(0, 0, 0);
        float count = 0;
        foreach (var pointOfZone in checkpoint.points){
            center += pointOfZone.pointGameObject.transform.position;
            count++;
        }
        var theCenter = center / count;
        Middle.transform.position = theCenter;
        Middle.name = checkpoint.name;
        
        Point centerPoint = new Point();
        centerPoint.pointGameObject = Middle;
        checkpoint.points.Add(centerPoint);
        return Middle;
    }

    void Start()
    {
        LoadJsonData();
        FillIndexes();
        // Nastavime drony
        SetupDrones();

        foreach(var checkpoint in mission.checkpoints){
            checkpoint.droneCount = checkpoint.drones.Count;
            // Drony dostanu misiu
            if(checkpoint.type == "regular"){
                GeneratePoint(checkpoint);
            } else if(checkpoint.type == "zone"){
                GenerateZone(checkpoint);
            }

            // Priradim kompetentnym dronom missiu
            foreach(var name in checkpoint.drones){
                foreach(var drone in mission.drones){
                    if(drone.name == name){
                        drone.checkpoints.Add(checkpoint);
                    }
                }
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