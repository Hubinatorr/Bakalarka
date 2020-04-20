/*
Author: Bc. Kamil Sedlmajer (kamilsedlmajer@gmail.com)

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Unity.Map;
using TMPro;

public class NavigationController : MonoBehaviour
{
    public GameObject target;
    public GameObject prefab;
    public AbstractMap Map;
    public GameObject Drone;

    private bool positionActual = false;
    private int PointCounter = 1;
    public Text mission;
    public GameObject HomeArrow;
    public GameObject NavigationArrow;

    public static List<GameObject> icons = new List<GameObject>();
    public TextMeshProUGUI HomeDistanceText;
    public TextMeshProUGUI NavigationDistanceText;

    public GameObject targetTransform;
    public GameObject HomePointObject;

    public Camera cam;
    //Demo 
    
    

    public Camera defcam;
    public Camera mapcam;
    // Start is called before the first frame update
    private bool _onScreen;

    private int i = 0;
   // public GameObject droneObject;
    private NavigationPoint HomePoint;
    private List<NavigationPoint> navigationPoints = new List<NavigationPoint>();
    private NavigationPoint activeNavigationPoint;

    private int state = 0;
    private Color[] colorArray = new Color[] {Color.red, Color.blue, Color.cyan, Color.magenta,  Color.yellow, new Color(1, 0.5f, 0.5f, 0.8f), new Color(1, 0.5f, 0f, 0.8f), Color.white};

    public List<NavigationPoint> getPoints() { return navigationPoints; }

    void Start()
    {
        mission.text = "Current Objective : Reach Point 1";
        HomePoint = new NavigationPoint(new Color(0,1,0, 0.8f), "Home", true, HomePointObject);

        /*
        NavigationPoint point1 = new NavigationPoint(new Color(1, 0, 0, 0.8f), "Point Red", true, HomePointObject);
        navigationPoints.Add(point1);
        NavigationPoint point2 = new NavigationPoint(new Color(1,1, 1, 0.8f), "Point White", true, HomePointObject);
        navigationPoints.Add(point2);
        NavigationPoint point3 = new NavigationPoint(new Color(1, 0.5f, 0.5f, 0.8f), "Point Pink", true, HomePointObject);
        navigationPoints.Add(point3);
        */
        Color col = colorArray[navigationPoints.Count % 8];
        GameObject WayPointPointerPrefab = Resources.Load<GameObject>("Zones/WayPointLOng");;

        GameObject WayPointPointer = Instantiate(WayPointPointerPrefab);
        WayPointPointer.transform.localPosition = new Vector3(200, 400,0);
        WayPointPointer.transform.parent = transform;
        //
        //
        Color c = new Color(col.r, col.g, col.b, 0.5f);
        WayPointPointer.GetComponent<Renderer>().material.color = c;

        NavigationPoint point = new NavigationPoint(col, "Point "+ PointCounter, true, WayPointPointer);
        PointCounter++;
        navigationPoints.Add(point);
        GameObject icon = (GameObject)Instantiate(prefab);
        icon.transform.SetParent(target.transform);
        Debug.Log ("k");
        icon.GetComponent<Image>().color = c;


        col = colorArray[navigationPoints.Count % 8];
        WayPointPointerPrefab = Resources.Load<GameObject>("Zones/WayPoint");;

        WayPointPointer = Instantiate(WayPointPointerPrefab);
        WayPointPointer.transform.localPosition = new Vector3(18, 225,100);
        WayPointPointer.transform.parent = transform;
        //
        //
        c = new Color(col.r, col.g, col.b, 0.5f);
        WayPointPointer.GetComponent<Renderer>().material.color = c;

        point = new NavigationPoint(col, "Point "+ PointCounter, true, WayPointPointer);
        PointCounter++;
        navigationPoints.Add(point);
        icon = (GameObject)Instantiate(prefab);
        icon.transform.SetParent(target.transform);
        Debug.Log ("k");
        icon.GetComponent<Image>().color = c;

        // zona
        col = colorArray[navigationPoints.Count % 8];
        WayPointPointerPrefab = Resources.Load<GameObject>("Zones/zona");;

        WayPointPointer = Instantiate(WayPointPointerPrefab);
        WayPointPointer.transform.localPosition = new Vector3(-120, 330,340);
        WayPointPointer.transform.localScale = new Vector3(10000, 10000, 10000);
        WayPointPointer.transform.parent = transform;
        //
        //
        c = new Color(col.r, col.g, col.b, 0.5f);
        WayPointPointer.GetComponent<Renderer>().material.color = c;

        point = new NavigationPoint(col, "Point "+ PointCounter, true, WayPointPointer);
        PointCounter++;
        navigationPoints.Add(point);
        icon = (GameObject)Instantiate(prefab);
        icon.transform.SetParent(target.transform);
        Debug.Log ("k");
        icon.GetComponent<Image>().color = c;
    }
    public void AddPoint(Vector3 position, bool onGround)
    {
        Color col = colorArray[navigationPoints.Count % 8];

        GameObject WayPointPointerPrefab;

        if(!onGround) WayPointPointerPrefab = Resources.Load<GameObject>("Zones/WayPointPointer");
        else WayPointPointerPrefab = Resources.Load<GameObject>("Zones/WayPoint");

        GameObject WayPointPointer = Instantiate(WayPointPointerPrefab);
        WayPointPointer.transform.localPosition = position;
        WayPointPointer.transform.parent = transform;
        //
        WayPointPointer.transform.localPosition = new Vector3(position.x, position.y,position.z);
        //
        Color c = new Color(col.r, col.g, col.b, 0.5f);
        WayPointPointer.GetComponent<Renderer>().material.color = c;

        NavigationPoint point = new NavigationPoint(col, "Point "+ PointCounter, onGround, WayPointPointer);
        PointCounter++;
        navigationPoints.Add(point);
        GameObject icon = (GameObject)Instantiate(prefab);
        icon.transform.SetParent(target.transform);
        Debug.Log ("k");
        icon.GetComponent<Image>().color = c;


    }

    public void DeletePoint(int index)
    {
        NavigationPoint point = navigationPoints[index];
        GameObject.Destroy(point.pointObject);
        navigationPoints.RemoveAt(index);
    }


        // Update is called once per frame
    void Update()
    {   
        int i = 0;
        int j = 0;
        if(state == 0)
        {
            mission.text = "Current Objective : Reach Point 1";
            if(Vector3.Distance(Drone.transform.position,transform.GetChild(1).transform.position) < 9.0){
                state = 1;
            }
            foreach( Transform child in  transform){
            if(i == 2 || i == 3)
                child.gameObject.SetActive(false);
            else
                child.gameObject.SetActive(true);
            i++;
            }
        } else if(state == 1){
            if(Vector3.Distance(Drone.transform.position,transform.GetChild(2).transform.position) < 1.0){
                state = 2;
            }
            mission.text = "Current Objective : Land On Point 2";
            foreach( Transform child in  transform){
            if(i == 1 || i == 3)
                child.gameObject.SetActive(false);
            else
                child.gameObject.SetActive(true);
            i++;
            }
        } else if(state == 2){
            Debug.Log(Vector3.Distance(Drone.transform.position,transform.GetChild(3).transform.position));
             if(Vector3.Distance(Drone.transform.position,transform.GetChild(3).transform.position) < 110.0){
                state = 3;
            }
             mission.text = "Current Objective : Reach Point 3";
            foreach( Transform child in  transform){
            if(i == 1 || i == 2)
                child.gameObject.SetActive(false);
            else
                child.gameObject.SetActive(true);
            i++;
            }
        } else if(state == 3){
            if(Vector3.Distance(Drone.transform.position,transform.GetChild(3).transform.position) > 200.0)
                state = 2;
            mission.text = "Current Objective :Observe";
            // foreach( Transform child in  transform){
            // if(i == 1 || i == 2 || i == 3)
            //     child.gameObject.SetActive(false);
            // else
            //     child.gameObject.SetActive(true);
            // i++;
        // }
        }
        
        

        if (!positionActual) //INICIALIZACE NA ZAČÁTKU. neni ale možné provest ve start, protože ještě není inicializovaná mapa
        {
            ChangeHomePosition(HomePoint.pointObject.transform.localPosition, true);
            positionActual = true;
        }

        HomeArrow.transform.LookAt(HomePoint.pointObject.transform);

        float dist = Vector3.Distance(HomeArrow.transform.position, HomePoint.pointObject.transform.position);
        HomeDistanceText.text = Mathf.Round(dist) + "m";

        if (activeNavigationPoint != null)
        {
            NavigationArrow.transform.LookAt(activeNavigationPoint.pointObject.transform);

            float dist2 = Vector3.Distance(NavigationArrow.transform.position, activeNavigationPoint.pointObject.transform.position);
            NavigationDistanceText.text =  Mathf.Round(dist2) + "m";
        }

        if(GuiController.isMap)
            cam = mapcam;
        else
            cam = defcam;

        foreach(Transform child in target.transform)
        {
            Vector3 iconPos = cam.WorldToScreenPoint(navigationPoints[i].pointObject.transform.position);
            _onScreen = cam.pixelRect.Contains( iconPos ) && iconPos.z > cam.nearClipPlane;
            if(_onScreen && i!= 0){
                child.gameObject.SetActive(true);
            } else {
                child.gameObject.SetActive(false);
            }

            if(_onScreen && i== 0 && GuiController.isMap)
                child.gameObject.SetActive(true);
            child.transform.position = iconPos;
            i++;
            if(child.transform.position != iconPos)
                child.transform.position = iconPos;

        }
         i=0;
    }

    //show and hide navigation arrow
    public bool ChangeHomeArrowActivity()
    {
        HomeArrow.SetActive(!HomeArrow.activeSelf);
        HomeDistanceText.gameObject.SetActive(HomeArrow.activeSelf);

        if (HomeArrow.activeSelf && activeNavigationPoint!=null)
            showBothArrows();
        else
            showOneArrow();

        return HomeArrow.activeSelf;
    }


        //for pointID=0 navigatios arrowwill be hidden
    public void showNavigationArrow(int pointID)
    {
        if(pointID>=0 && navigationPoints[pointID]!=null)
        {
            NavigationArrow.SetActive(true);
            NavigationDistanceText.gameObject.SetActive(true);

            activeNavigationPoint = navigationPoints[pointID];

            //teď se probourat dovnitř a nastavit barvičky jednotlivých komponent
            NavigationArrow.transform.GetChild(0).Find("arrow").gameObject.GetComponent<Renderer>().material.color = activeNavigationPoint.color;
            NavigationArrow.transform.GetChild(0).Find("backside").gameObject.GetComponent<Renderer>().material.color = activeNavigationPoint.backsideColor;

            if (HomeArrow.activeSelf)
                showBothArrows();
            else
                showOneArrow();
        }
        else
        {
            NavigationArrow.SetActive(false);
            NavigationDistanceText.gameObject.SetActive(false);
            activeNavigationPoint = null;
            showOneArrow();  
        }
    }

    

    //pozice se zadává v unity Units
    public void ChangeHomePosition(Vector3 position, bool onGround)
    {
        if (HomePoint.onGround)
        {
            float groundAltitude = Map.QueryElevationInUnityUnitsAt(Map.WorldToGeoPosition(position));
            HomePoint.pointObject.transform.localPosition = new Vector3(position.x, groundAltitude, position.z);
        }
        else
            HomePoint.pointObject.transform.localPosition = position;

        HomePoint.onGround = onGround;
    }

    public Vector3 getHomePosition()
    {
        return transform.localPosition;
    }


    protected void showOneArrow()
    {
        HomeArrow.transform.localPosition = new Vector3(0, HomeArrow.transform.localPosition.y, HomeArrow.transform.localPosition.z);
        NavigationArrow.transform.localPosition = new Vector3(0, NavigationArrow.transform.localPosition.y, NavigationArrow.transform.localPosition.z);

        Vector3 homeTextPos = HomeDistanceText.GetComponent<RectTransform>().anchoredPosition;
        HomeDistanceText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, homeTextPos.y, homeTextPos.z);

        Vector3 navPointTextPos = NavigationDistanceText.GetComponent<RectTransform>().anchoredPosition;
        NavigationDistanceText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, navPointTextPos.y, navPointTextPos.z);
    }

    protected void showBothArrows()
    {
        HomeArrow.transform.localPosition = new Vector3(-0.1f, HomeArrow.transform.localPosition.y, HomeArrow.transform.localPosition.z);
        NavigationArrow.transform.localPosition = new Vector3(0.1f, NavigationArrow.transform.localPosition.y, NavigationArrow.transform.localPosition.z);

        Vector3 homeTextPos = HomeDistanceText.GetComponent<RectTransform>().anchoredPosition;
        HomeDistanceText.GetComponent<RectTransform>().anchoredPosition = new Vector3(-30,homeTextPos.y, homeTextPos.z);

        Vector3 navPointTextPos = NavigationDistanceText.GetComponent<RectTransform>().anchoredPosition;
        NavigationDistanceText.GetComponent<RectTransform>().anchoredPosition = new Vector3(30, navPointTextPos.y, navPointTextPos.z);
    }
}
